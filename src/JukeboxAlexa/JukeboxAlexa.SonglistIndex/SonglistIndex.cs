using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.SonglistIndex {
    public class SonglistIndex {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider DynamodbProvider;

        //--- Constructor ---
        public SonglistIndex(IDynamodbDependencyProvider awsDynmodbProvider) {
            DynamodbProvider = awsDynmodbProvider;
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

            var splitSongTitle = title.S.Split(" ");
            foreach (var wordFromList in splitSongTitle) {

                var word = wordFromList.ToLower();
                
                // key    
                var recordKey = new Dictionary<string, AttributeValue> {
                    {
                        "word", new AttributeValue {
                            S = word
                        }
                    }
                };
                
                // Find existing songs in database for that word
                var existingSongsInDb = await GetExistingSongs(recordKey).ConfigureAwait(false);
                LambdaLogger.Log($"EXISTING SONGS IN DATABASE: {JsonConvert.SerializeObject(existingSongsInDb)}");
                
                var songIndexInExistingSongs = GetIndexOfThisSong(existingSongsInDb, songItem);
                LambdaLogger.Log($"songIndexInExistingSongs: {songIndexInExistingSongs}");
                
                switch (action) {
                case "INSERT": {
                    LambdaLogger.Log($"INDEXING WORD: {word}");
                    await InsertSong(existingSongsInDb, songIndexInExistingSongs, recordKey, songItem).ConfigureAwait(false);
                }
                break;
                case "REMOVE": {
                    LambdaLogger.Log($"INDEXING WORD: {word}");
                    await DeleteSong(existingSongsInDb, songIndexInExistingSongs, recordKey).ConfigureAwait(false);
                }
                break;
                }
            }
        }

        public async Task InsertSong(IList<SongModel.Song> existingSongsInDb, int songIndexInExistingSongs, IDictionary<string, AttributeValue> key, SongModel.Song songToInsert) {
           
            // other info
            var totalExistingSongs = existingSongsInDb.Count;
            LambdaLogger.Log($"totalExistingSongs: {totalExistingSongs}");
            
            // no record for that word
            if (totalExistingSongs == 0) {
                LambdaLogger.Log($"ADDING NEW ENTRY FOR SONG: {JsonConvert.SerializeObject(songToInsert)}");
                await AddWordSongs(key.ToDictionary(x => x.Key, x => x.Value), songToInsert).ConfigureAwait(false);
            }
            // existing record for that word and but song doesn't exist
            else if (totalExistingSongs > 0 && songIndexInExistingSongs == -1) {
                LambdaLogger.Log($"INSERTING ENTRY FOR WORD: {JsonConvert.SerializeObject(songToInsert)}");
                await InsertWordSongs(key.ToDictionary(x => x.Key, x => x.Value), songToInsert, totalExistingSongs).ConfigureAwait(false);                
            }
            // song found in list, update that item
            else if (totalExistingSongs > 0 && songIndexInExistingSongs >= 0) {
                LambdaLogger.Log($"UPDATING ENTRY FOR WORD: {JsonConvert.SerializeObject(songToInsert)}");
                await InsertWordSongs(key.ToDictionary(x => x.Key, x => x.Value), songToInsert, songIndexInExistingSongs).ConfigureAwait(false);
            }
        }

        public async Task DeleteSong(IList<SongModel.Song> existingSongsInDb, int songIndexInExistingSongs, IDictionary<string, AttributeValue> key) {
          
            // other info
            var totalExistingSongs = existingSongsInDb.Count;
            LambdaLogger.Log($"totalExistingSongs: {totalExistingSongs}");
            
            if (totalExistingSongs >= 0 && songIndexInExistingSongs == -1) {
                LambdaLogger.Log("SONG NOT FOUND FOR THE WORD");
            }
            // there are mutliple records for that word
            else if (totalExistingSongs >= 1 && songIndexInExistingSongs >= 0) {
                LambdaLogger.Log($"REMOVING SONG FROM INDEX: {songIndexInExistingSongs}");
                await RemoveWordSongs(key.ToDictionary(x => x.Key, x => x.Value), songIndexInExistingSongs).ConfigureAwait(false);
            }
        }

        public async Task RemoveWordSongs(Dictionary<string, AttributeValue> recordKey, int songIndexInExistingSongs) {
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();
            var updateExpression = $"REMOVE songs[{songIndexInExistingSongs}]";
            await DynamodbProvider.DynamodbUpdateItemAsync(recordKey, updateExpression, expressionAttributeValues);
        }
        
        public async Task AddWordSongs(Dictionary<string, AttributeValue> recordKey, SongModel.Song songToInsert) {
            var expressionAttributeValues = new Dictionary<string, AttributeValue> {
                {
                    ":n", new AttributeValue {
                        L = new List<AttributeValue> {
                            new AttributeValue {
                                S = JsonConvert.SerializeObject(songToInsert)
                            }   
                        }
                    }
                }
            };
            var updateExpression = "SET songs = :n";
            await DynamodbProvider.DynamodbUpdateItemAsync(recordKey, updateExpression, expressionAttributeValues);
        }
        
        public async Task InsertWordSongs(Dictionary<string, AttributeValue> recordKey, SongModel.Song songToInsert, int songIndexInExistingSongs) {
            var expressionAttributeValues = new Dictionary<string, AttributeValue> {
                {
                    ":n", new AttributeValue {
                        S = JsonConvert.SerializeObject(songToInsert)
                    }   
                }
            };
            var updateExpression = $"SET songs[{songIndexInExistingSongs}] = :n";
            await DynamodbProvider.DynamodbUpdateItemAsync(recordKey, updateExpression, expressionAttributeValues);
        }
        
        public async Task<IList<SongModel.Song>> GetExistingSongs(Dictionary<string, AttributeValue> recordKey) {
            var existingSongs = new List<SongModel.Song>();
            var dbItems = await DynamodbProvider.DynamodbGetItemAsync(recordKey);
            LambdaLogger.Log($"dbItems: {JsonConvert.SerializeObject(dbItems)}");
            foreach (var dbItem in dbItems.Item) {
                if (dbItem.Key != "songs") {
                    continue;
                }
                foreach (var song in dbItem.Value.L) {
                    existingSongs.Add(JsonConvert.DeserializeObject<SongModel.Song>(song.S));
                }
            }
            return existingSongs;
        }

        public int GetIndexOfThisSong(IList<SongModel.Song> existingSongList, SongModel.Song songFromRequest) {
            
            // find this song in existing items -- return the index
            var index = 0;
            foreach (var existingSong in existingSongList) {
                if (existingSong.Title == songFromRequest.Title && existingSong.Artist == songFromRequest.Artist && existingSong.SongNumber == songFromRequest.SongNumber) {
                    LambdaLogger.Log($"FOUND SONG INDEX: {index}");
                    return index;
                }
                index += 1;
            }
            return -1;
        }
    }
}