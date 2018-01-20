using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library
{
    public class JukeboxCacheDynamoDb
    {
        
        //--- Fields ---
        private static AmazonDynamoDBClient _dynamoClient;
        private static string _tableName;

        //--- Constructors ---
        public JukeboxCacheDynamoDb()
        {
            _dynamoClient = new AmazonDynamoDBClient();
            _tableName = "JukeboxSongsCache";
        }
        
        //--- Methods ---
        public List<SongModel.SongCache> FindWordsFromCache(string title)
        {
            Console.WriteLine($"***INFO***: QueryWordFromCache for `{title}`");
            
            // get the list of words from the database
            var getItemRequest = GenerateBatchGetItemRequest(title);
            BatchGetItemResponse queryResponse = _dynamoClient.BatchGetItemAsync(getItemRequest).Result;
            Console.WriteLine($"***INFO***: queryResponse for `{JsonConvert.SerializeObject(queryResponse)}`");

            // process the results for easy use
            var songsFromDatabaseCache = GetSongsFromDatabaseCacheResponse(queryResponse);
            return songsFromDatabaseCache;
        }

        private BatchGetItemRequest GenerateBatchGetItemRequest(string title)
        {
            var splitTitle = title.ToLower().Split(' ');
            
            // Create a list of words to query
            var titleWordList = splitTitle
                .Select(word => new Dictionary<string, AttributeValue>
                {
                    {"word", new AttributeValue {S = word}}
                })
                .ToList();
            
            return new BatchGetItemRequest
            {
                RequestItems = new Dictionary<string, KeysAndAttributes>
                {
                    {_tableName, new KeysAndAttributes {Keys = titleWordList}}
                },
                ReturnConsumedCapacity = "NONE",

            };
        }
        
        private static List<SongModel.SongCache> GetSongsFromDatabaseCacheResponse(BatchGetItemResponse queryResponse)
        {
            var songTallyList = new List<SongModel.SongCache>();
            foreach (var result in queryResponse.Responses["JukeboxSongsCache"])
            {
               
                // song object from found result
                Console.WriteLine($"***INFO***: result `{JsonConvert.SerializeObject(result)}`");
                var song = ParseSongFromDatabaseResponse(result);
                
                // initialize variables
                var foundSongInTallyList = FindSongInTallyList(songTallyList, song);
                if (foundSongInTallyList != null)
                {
                    // add one to the song's counter in tally list
                    int.TryParse(foundSongInTallyList.Item1.Count, out int currentCount);
                    var count = currentCount + 1;
                    foundSongInTallyList.Item1.Count = count.ToString();
                    songTallyList[foundSongInTallyList.Item2] = foundSongInTallyList.Item1;
                }
                else
                {
                    // new song in the tally list
                    var songCache = new SongModel.SongCache
                    {
                        Count = "1",
                        Song = song
                    };
                    songTallyList.Add(songCache);
                }
            }
            Console.WriteLine($"***INFO***: songTallyList `{JsonConvert.SerializeObject(songTallyList)}`");
            return songTallyList;

        }

        private static SongModel.Song ParseSongFromDatabaseResponse(IReadOnlyDictionary<string, AttributeValue> result)
        {
            Console.WriteLine($"***INFO***: songTallyList `{JsonConvert.SerializeObject(result)}`");
            var song = new SongModel.Song
            {
                Title = result["songs"].L.FirstOrDefault().M["title"].S,
                Artist = result["songs"].L.FirstOrDefault().M["artist"].S,
                TrackNumber = result["songs"].L.FirstOrDefault().M["track_number"].S
            };
            return song;
        }
        
        private static Tuple<SongModel.SongCache, int> FindSongInTallyList(IReadOnlyCollection<SongModel.SongCache> songTallyList, SongModel.Song song)
        {
            if (!songTallyList.Any()) return null;
            var foundSongs = songTallyList
                .Select((k, index)=> new {k, index})
                .FirstOrDefault(k2 => (
                    k2.k.Song.Title == song.Title &&
                    k2.k.Song.Artist == song.Artist &&
                    k2.k.Song.TrackNumber == song.TrackNumber));
            if (foundSongs == null) return null;
            Console.WriteLine($"***INFO***: QueryWordFromCache foundSongskey for `{JsonConvert.SerializeObject(foundSongs)}`");
            return Tuple.Create(foundSongs.k, foundSongs.index);
        }
    }
}