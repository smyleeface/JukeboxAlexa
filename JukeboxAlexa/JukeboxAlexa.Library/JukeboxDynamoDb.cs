using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library {
    public class JukeboxDynamoDb {
        
        //--- Fields ---
        private IAmazonDynamoDB _dynamoClient;
        private string _tableName;
        private string _indexSearchTitle;
        private string _indexSearchTitleArtist;

        //--- Constructors ---
        public JukeboxDynamoDb(IAmazonDynamoDB dynamodbClient, string tableName, string indexNameSearchTitle, string indexNameSearchTitleArtist) {
            _dynamoClient = dynamodbClient;
            _tableName = tableName;
            _indexSearchTitle = indexNameSearchTitle;
            _indexSearchTitleArtist = indexNameSearchTitleArtist;
        }
        
        //--- Methods ---
        public async Task<IEnumerable<SongModel.Song>> FindSongsByTitleArtistAsync(string title, string artist) {
            LambdaLogger.Log($"*** INFO: FindSongsByTitleArtist for `{title}` and `{artist}");
            var queryRequest = QueryRequestTitleArtist(title, artist);
            var queryResponse = await _dynamoClient.QueryAsync(queryRequest);
            LambdaLogger.Log($"*** INFO: queryResponse `{JsonConvert.SerializeObject(queryResponse)}`");
            var parsedSongList = ParseSongsFromDatabaseResponse(queryResponse);
            return parsedSongList;
        }
        
        public async Task<IEnumerable<SongModel.Song>> FindSongsByTitleAsync(string title) {
            LambdaLogger.Log($"*** INFO: FindSongsByTitle for `{title}`");
            var queryRequest = QueryRequestTitle(title);
            var queryResponse = await _dynamoClient.QueryAsync(queryRequest);
            LambdaLogger.Log($"*** INFO: queryResponse `{JsonConvert.SerializeObject(queryResponse)}`");
            var parsedSongList = ParseSongsFromDatabaseResponse(queryResponse);
            return parsedSongList;
        }

        public async Task<IEnumerable<SongModel.Song>> FindSongsByNumberAsync(string trackNumber) {
            LambdaLogger.Log($"*** INFO: FindSongsByNumber for `{trackNumber}`");
            var queryRequest = QueryRequestNumber(trackNumber);                    
            var queryResponse = await _dynamoClient.QueryAsync(queryRequest);
            LambdaLogger.Log($"*** INFO: queryResponse `{JsonConvert.SerializeObject(queryResponse)}`");
            var parsedSongList = ParseSongsFromDatabaseResponse(queryResponse);
            return parsedSongList; 
        }

        public async Task<ScanResponse> ScanAsync() {
            var scanRequest = new ScanRequest {
                TableName = _tableName
            };
            return await _dynamoClient.ScanAsync(scanRequest);
        }
        
        public QueryRequest QueryRequestTitleArtist(string title, string artist) {
            return new QueryRequest {
                TableName = _tableName,
                IndexName = _indexSearchTitleArtist,
                KeyConditionExpression = "search_title = :v_song AND search_artist = :v_artist",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_song", new AttributeValue { S = title.ToLower() }},
                    {":v_artist", new AttributeValue { S = artist.ToLower() }}
                }
            };
        }

        public QueryRequest QueryRequestTitle(string title) {
            return new QueryRequest {
                TableName = _tableName,
                IndexName = _indexSearchTitle,
                KeyConditionExpression = "search_title = :v_song",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_song", new AttributeValue { S = title.ToLower() }}}
            };
        }
        
        public QueryRequest QueryRequestNumber(string songNumber) {
            return new QueryRequest {
                TableName = _tableName,
                KeyConditionExpression = "track_number = :v_number",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_number", new AttributeValue { S = songNumber }}}
            };
        }
        
        public IEnumerable<SongModel.Song> ParseSongsFromDatabaseResponse(QueryResponse queryResponse) {
            var parsedSongList = new List<SongModel.Song>();
            foreach (Dictionary<string, AttributeValue> item in queryResponse.Items) {
                var song = new SongModel.Song();
                foreach (KeyValuePair<string, AttributeValue> attribute in item) {
                    var attributeValue = attribute.Value.S;
                    switch (attribute.Key) {
                        case "title":
                            song.Title = attributeValue;
                            break;
                            
                        case "artist":
                            song.Artist = attributeValue;
                            break;
                        
                        case "track_number":
                            song.Number = attributeValue;
                            break;
                    }
                }
                parsedSongList.Add(song);
            }
            LambdaLogger.Log($"*** INFO: {JsonConvert.SerializeObject(parsedSongList)}");
            return parsedSongList;
        }

        public async Task<BatchWriteItemResponse> BatchWriteItemAsync(IEnumerable<WriteRequest> writeRequests) {
            return await _dynamoClient.BatchWriteItemAsync(new BatchWriteItemRequest {
                RequestItems = new Dictionary<string, List<WriteRequest>> {
                    { _tableName, writeRequests.ToList() }
                }
            });
        }
    }
}