using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using JukeboxAlexa.Library;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa.SonglistIndex {
    public class Function : IDynamodbDependencyProvider  {
        
        //--- Fields ---
        private readonly SonglistIndex _songlistUpload;
        private readonly JukeboxDynamoDb _jukeboxDynamoDb;
        
        
        //--- Constructors ---
        public Function() {
            var tableName = Environment.GetEnvironmentVariable("STR_DYNAMODBSONGS");
            var indexNameSearchTitle = Environment.GetEnvironmentVariable("INDEX_NAME_SEARCH_TITLE");
            var indexNameSearchTitleArtist = Environment.GetEnvironmentVariable("INDEX_NAME_SEARCH_TITLE_ARTIST");
            var indexTableName = Environment.GetEnvironmentVariable("STR_DYNAMODBTITLEWORDCACHE");
            _jukeboxDynamoDb = new JukeboxDynamoDb(new AmazonDynamoDBClient(), tableName, indexNameSearchTitle, indexNameSearchTitleArtist, indexTableName);
            _songlistUpload = new SonglistIndex(this);
        }

        //--- FunctionHandler ---
        public async Task FunctionHandlerAsync(DynamoDBEvent dynamoDbEvent, ILambdaContext context) {
            LambdaLogger.Log($"*** INFO: Event: {JsonConvert.SerializeObject(dynamoDbEvent)}");
            
            // process request
            try {
                await _songlistUpload.HandleRequest(dynamoDbEvent.Records.FirstOrDefault());
            }
            catch (Exception e) {
                LambdaLogger.Log($"Exception occured: {e}");
            }
        }
        
        Task<GetItemResponse> IDynamodbDependencyProvider.DynamodbGetItemAsync(IDictionary<string, AttributeValue> key) => _jukeboxDynamoDb.GetItemAsync(key);
        Task<UpdateItemResponse> IDynamodbDependencyProvider.DynamodbUpdateItemAsync(Dictionary<string, AttributeValue> key, string updateExpression, IDictionary<string, AttributeValue> expressionAttributeValues) => _jukeboxDynamoDb.UpdateItemAsync(key, updateExpression, expressionAttributeValues);
    }
}
