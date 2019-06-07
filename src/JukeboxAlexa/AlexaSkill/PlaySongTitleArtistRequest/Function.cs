using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using LambdaSharp;
using LambdaSharp.ApiGateway;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa.PlaySongTitleArtistRequest {
    public class Function : ALambdaApiGatewayFunction, ICommonDependencyProvider, IDynamodbDependencyProvider  {
        
        //--- Fields ---
        private PlaySongTitleArtistRequest _playSongArtistRequest;
        private JukeboxDynamoDb _jukeboxDynamoDb;

        //--- Constructors ---
        public override Task InitializeAsync(LambdaConfig config) {
            var queueName = config.ReadText("SqsSongQueue");
            var tableName = AwsConverters.ConvertDynamoDBArnToName(config.ReadText("DynamoDbSongs"));
            var indexNameSearchTitle = config.ReadText("DynamoDbIndexNameSearchTitleName");
            var indexNameSearchTitleArtist = config.ReadText("DynamoDbIndexNameSearchTitleArtistName");
            var indexTableName = AwsConverters.ConvertDynamoDBArnToName(config.ReadText("DynamoDbTitleWordCache"));
            _jukeboxDynamoDb = new JukeboxDynamoDb(new AmazonDynamoDBClient(), tableName, indexNameSearchTitle, indexNameSearchTitleArtist, indexTableName);
            _playSongArtistRequest = new PlaySongTitleArtistRequest(this, new AmazonSQSClient(), queueName, this);
            return Task.CompletedTask;
        }

        //--- FunctionHandler ---
        public override async Task<APIGatewayProxyResponse> ProcessProxyRequestAsync(APIGatewayProxyRequest inputRequest) {
            LambdaLogger.Log($"*** INFO: API Request input from user: {JsonConvert.SerializeObject(inputRequest)}");
            var body = inputRequest.Body;
            LambdaLogger.Log($"*** INFO: API Request body from user: {body}");
            var input = JsonConvert.DeserializeObject<CustomSkillRequest>(body);
            LambdaLogger.Log($"*** INFO: Request input from user: {JsonConvert.SerializeObject(input)}");
    
            // process request
            var requestResult = await _playSongArtistRequest.HandleRequest(input);
            var response = new APIGatewayProxyResponse {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(requestResult),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" }
                }
            };
            return response;
        }
        
        string ICommonDependencyProvider.DateNow() => new DateTime().ToUniversalTime().ToString("yy-MM-ddHH:mm:ss");
        Task<IEnumerable<SongModel.Song>> IDynamodbDependencyProvider.DynamoDbFindSongsByTitleArtistAsync(string title, string artist) => _jukeboxDynamoDb.FindSongsByTitleArtistAsync(title, artist);
    }
}
