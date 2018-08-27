using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Castle.Core.Internal;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library {
    public class JukeboxDynamoDb {
        
        //--- Fields ---
        private IAmazonDynamoDB _dynamoClient;
        private string _songTableName;
        private string _songIndexSearchTitle;
        private string _songIndexSearchTitleArtist;
        private string _songWordIndexTableName;

        //--- Constructors ---
        public JukeboxDynamoDb(IAmazonDynamoDB dynamodbClient, string songTableName, string songIndexNameSearchTitle, string songIndexNameSearchTitleArtist, string songWordIndexTableName) {
            _dynamoClient = dynamodbClient;
            _songTableName = songTableName;
            _songIndexSearchTitle = songIndexNameSearchTitle;
            _songIndexSearchTitleArtist = songIndexNameSearchTitleArtist;
            _songWordIndexTableName = songWordIndexTableName;
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

        public async Task<IEnumerable<SongModel.Song>> FindSongsByNumberAsync(string songNumber) {
            LambdaLogger.Log($"*** INFO: FindSongsByNumber for `{songNumber}`");
            var queryRequest = QueryRequestNumber(songNumber);                    
            var queryResponse = await _dynamoClient.QueryAsync(queryRequest);
            LambdaLogger.Log($"*** INFO: queryResponse `{JsonConvert.SerializeObject(queryResponse)}`");
            var parsedSongList = ParseSongsFromDatabaseResponse(queryResponse);
            return parsedSongList; 
        }

        public async Task<ScanResponse> ScanAsync() {
            var scanRequest = new ScanRequest {
                TableName = _songTableName
            };
            return await _dynamoClient.ScanAsync(scanRequest);
        }
        
        public QueryRequest QueryRequestTitleArtist(string title, string artist) {
            return new QueryRequest {
                TableName = _songTableName,
                IndexName = _songIndexSearchTitleArtist,
                KeyConditionExpression = "search_title = :v_song AND search_artist = :v_artist",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_song", new AttributeValue { S = title.ToLower() }},
                    {":v_artist", new AttributeValue { S = artist.ToLower() }}
                }
            };
        }

        public QueryRequest QueryRequestTitle(string title) {
            return new QueryRequest {
                TableName = _songTableName,
                IndexName = _songIndexSearchTitle,
                KeyConditionExpression = "search_title = :v_song",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_song", new AttributeValue { S = title.ToLower() }}}
            };
        }
        
        public QueryRequest QueryRequestNumber(string songNumber) {
            return new QueryRequest {
                TableName = _songTableName,
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
                            song.SongNumber = attributeValue;
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
                    { _songTableName, writeRequests.ToList() }
                }
            });
        }

        //--- Actions for Song Index table ---
        public async Task<GetItemResponse> GetItemAsync(IDictionary<string, AttributeValue> key) {
            var getItemRequest = new GetItemRequest {
                TableName = _songWordIndexTableName,
                Key = key.ToDictionary(x => x.Key, x => x.Value)
            };
            return await _dynamoClient.GetItemAsync(getItemRequest);
        }

        public async Task<UpdateItemResponse> UpdateItemAsync(Dictionary<string, AttributeValue> key, string updateExpression, IDictionary<string, AttributeValue> expressionAttributeValues) {
            LambdaLogger.Log(updateExpression);
            var updateItemRequest = new UpdateItemRequest {
                TableName = _songWordIndexTableName,
                Key = key,
                UpdateExpression = updateExpression,
                ReturnValues = "NONE"
            };
            if (!expressionAttributeValues.IsNullOrEmpty()) {
                updateItemRequest.ExpressionAttributeValues = new Dictionary<string, AttributeValue>(expressionAttributeValues);
            }
            return await _dynamoClient.UpdateItemAsync(updateItemRequest);
        }
    }
}