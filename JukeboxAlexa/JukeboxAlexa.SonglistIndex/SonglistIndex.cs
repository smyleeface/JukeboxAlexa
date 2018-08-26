using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Castle.Core.Internal;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using JukeboxAlexa.SonglistIndex;
using Newtonsoft.Json;

namespace JukeboxAlexa.SonglistIndex {
    public class SonglistIndex {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider dynamodbProvider;
        public string bucketName;
        public string keyName;
        public IEnumerable<SongCsvModel> newSongs;
        public IEnumerable<SongCsvModel> oldSongs;
        public IEnumerable<SongCsvModel> songsToAdd;
        public IEnumerable<SongCsvModel> songsToDelete;

        //--- Constructor ---
        public SonglistIndex(IDynamodbDependencyProvider awsDynmodbProvider) {
            dynamodbProvider = awsDynmodbProvider;
        }

        //--- Methods ---
        public async Task HandleRequest(DynamoDBEvent.DynamodbStreamRecord dynamodbStreamRecord) {
            var action = dynamodbStreamRecord.EventName.Value;
            var requestItem = new Dictionary<string, AttributeValue>();
            LambdaLogger.Log($"Action: {action}");
            switch (action) {
                case "INSERT": {
                    requestItem = dynamodbStreamRecord.Dynamodb.NewImage;
                    break;
                }
                case "REMOVE": {
                    requestItem = dynamodbStreamRecord.Dynamodb.OldImage;
                    break;
                }
            }
            LambdaLogger.Log($"Request Item: {JsonConvert.SerializeObject(requestItem)}");
            requestItem.TryGetValue("song_number", out var number);
            requestItem.TryGetValue("artist", out var artist);
            requestItem.TryGetValue("title", out var title);
            
            // song to remove from index
            var songItem = new SongModel.Song {
                Artist = artist.S,
                Title = title.S,
                SongNumber = number.S
            };
            LambdaLogger.Log($"INDEXED SONG ITEM: {JsonConvert.SerializeObject(songItem)}");
            
            switch (action) {
                case "INSERT": {
                    var splitSongTitle = title.S.Split(" ");
                    foreach (var word in splitSongTitle) {
                        LambdaLogger.Log($"INDEXING WORD: {word}");
                        await InsertSong(word.ToLower(), title.S, artist.S, number.S, songItem);
                    }
                    break;
                }
                case "REMOVE": {
                    var splitSongTitle = title.S.Split(" ");
                    foreach (var word in splitSongTitle) {
                        LambdaLogger.Log($"INDEXING WORD: {word}");
                        await DeleteSong(word, title.S, artist.S, number.S, songItem);
                    }
                    break;
                }
            }
        }

        public async Task InsertSong(string word, string title, string artist, string number, SongModel.Song songToInsert) {
            
            // Find existing songs in database for that word
            var indexedSongsForWord = await GetExistingSongs(word);
            LambdaLogger.Log($"INDEXED SONGS FOR WORD: {JsonConvert.SerializeObject(indexedSongsForWord)}");
            
            // put object
            var recordKey = new Dictionary<string, AttributeValue> {
                {
                    "word", new AttributeValue {
                        S = word
                    }
                }
            };
            
            // other info
            var totalExistingSongs = indexedSongsForWord.Count;
            var songIndexInExistingSongs = GetIndexOfThisSong(indexedSongsForWord, title, artist, number);
            
            LambdaLogger.Log($"songIndexInExistingSongs: {songIndexInExistingSongs}");
            LambdaLogger.Log($"totalExistingSongs: {totalExistingSongs}");
            
            // no record for that word and no song exists
            if (totalExistingSongs >= 0 && songIndexInExistingSongs == -1) {
                LambdaLogger.Log($"ADDING NEW ENTRY FOR WORD: {word}");
                indexedSongsForWord.Add(JsonConvert.SerializeObject(songToInsert));
                await AddWordSongToDb(recordKey, songToInsert, indexedSongsForWord);
            }
            // there are other records for that word
            else if (totalExistingSongs > 0 && songIndexInExistingSongs >= 0) {
                LambdaLogger.Log($"UPDATING ENTRY FOR WORD: {word}");
                LambdaLogger.Log($"SONG INDEX: {songIndexInExistingSongs}");
                var updateExpression = $"SET songs[{songIndexInExistingSongs}] = :n";
                await UpdateAddWordSongsInDb(recordKey, songToInsert, updateExpression);
            }
        }
        
        public async Task DeleteSong(string word, string title, string artist, string number, SongModel.Song songToRemove) {
          
            // Find existing songs in database for that word
            var indexedSongsForWord = await GetExistingSongs(word);
            LambdaLogger.Log($"INDEXED SONGS FOR WORD: {JsonConvert.SerializeObject(indexedSongsForWord)}");
            
            // put object
            var recordKey = new Dictionary<string, AttributeValue> {
                {
                    "word", new AttributeValue {
                        S = word
                    }
                }
            };
            
            // other info
            var totalExistingSongs = indexedSongsForWord.Count;
            var songIndexInExistingSongs = GetIndexOfThisSong(indexedSongsForWord, title, artist, number);
           
            LambdaLogger.Log($"songIndexInExistingSongs: {songIndexInExistingSongs}");
            LambdaLogger.Log($"totalExistingSongs: {totalExistingSongs}");
            
            if (totalExistingSongs >= 0 && songIndexInExistingSongs == -1) {
                LambdaLogger.Log($"SONG NOT FOUND FOR THE WORD: {word}");
            }
            else if (totalExistingSongs == 1 && songIndexInExistingSongs == 0) {
                LambdaLogger.Log($"REMOVING WORD: {word}");
                await DeleteWordInDb(recordKey);
                //this workds
            }
            else if (totalExistingSongs >= 1 && songIndexInExistingSongs >= 0) {
                // there are mutliple records for that word
                LambdaLogger.Log($"REMOVING SONG IN THE WORD: {word}");
                LambdaLogger.Log($"SONG INDEX: {songIndexInExistingSongs}");
                
                // NOTE (pattyr, 20180826): couldn't get REMOVE songs[{songIndexInExistingSongs}] to work
                // Filter the song to remove
                var counter = 0;
                var newListOfSongs = new List<string>();
                foreach (var songDetail in indexedSongsForWord) {
                    if (songIndexInExistingSongs != counter) {   
                        newListOfSongs.Add(songDetail);
                    }
                    counter += 1;
                }
                LambdaLogger.Log($"INDEXED SONGS REMOVED FOR WORD: {JsonConvert.SerializeObject(newListOfSongs)}");
                await DeleteWordInDb(recordKey);
                await AddWordSongToDb(recordKey, songToRemove, newListOfSongs);
            }
        }

        public async Task<IList<string>> GetExistingSongs(string word) {
            var getItemKey = new Dictionary<string, AttributeValue> {
                {
                    "word", new AttributeValue {
                        S = word
                    }
                }
            };
            var dbItems = await dynamodbProvider.DynamodbGetItemAsync(getItemKey);
            foreach (var dbItem in dbItems.Item) {
                if (dbItem.Key != "songs") continue;
                return dbItem.Value.SS.ToList();
            }
            return new List<string>();
        }

        public async Task UpdateAddWordSongsInDb(Dictionary<string, AttributeValue> songItem, SongModel.Song song, string updateExpression) {
            var expressionAttributeValues = new Dictionary<string, AttributeValue> {
                {
                    ":n", new AttributeValue {
                        S = JsonConvert.SerializeObject(song)
                    }
                }
            };
            await dynamodbProvider.DynamodbUpdateItemAsync(songItem, updateExpression, expressionAttributeValues);
        }
        
        public async Task UpdateRemoveWordSongsInDb(Dictionary<string, AttributeValue> songItem, string updateExpression) {
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();
            await dynamodbProvider.DynamodbUpdateItemAsync(songItem, updateExpression, expressionAttributeValues);
        }

        public async Task DeleteWordInDb(IDictionary<string, AttributeValue> key) {
            await dynamodbProvider.DynamodbDeleteWordInDbAsync(key);
        }

        public async Task AddWordSongToDb(IDictionary<string, AttributeValue> songItem, SongModel.Song song, IList<string> indexedSongs) {
            songItem.Add("songs", new AttributeValue {
                SS = indexedSongs.Distinct().ToList()
            });                    
            await dynamodbProvider.DynamodbPutItemAsync(songItem);
        }

        public int GetIndexOfThisSong(IList<string> songList, string title, string artist, string number) {
            
            // find this song in existing items -- return the index
            var index = 0;
            foreach (var song in songList) {
                var deserializedSong = JsonConvert.DeserializeObject<SongModel.Song>(song);
                if (deserializedSong.Title == title && deserializedSong.Artist == artist && deserializedSong.SongNumber == number) {
                    LambdaLogger.Log($"FOUND SONG INDEX: {index}");
                    return index;
                }
                index += 1;
            }
            return -1;
        }
    }
}