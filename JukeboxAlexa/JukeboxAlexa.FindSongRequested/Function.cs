using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.SQS;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa.FindSongRequested
{
    public class Function : ICommonDependencyProvider, IDynamodbDependencyProvider  {
        
        //--- Fields ---
        private readonly FindSongRequested _playSongRequest;
        private readonly JukeboxDynamoDb _jukeboxDynamoDb;

        //--- Constructors ---
        public Function() {
            var queueName = Environment.GetEnvironmentVariable("JukeboxQueueName");
            var tableName = Environment.GetEnvironmentVariable("JukeboxTableName");
            var indexNameSearchTitle = Environment.GetEnvironmentVariable("indexNameSearchTitle");
            var indexNameSearchTitleArtist = Environment.GetEnvironmentVariable("indexNameSearchTitleArtist");
            _jukeboxDynamoDb = new JukeboxDynamoDb(new AmazonDynamoDBClient(), tableName, indexNameSearchTitle, indexNameSearchTitleArtist);
            _playSongRequest = new FindSongRequested(this, new AmazonSQSClient(), this);
        }

        //--- FunctionHandler ---
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context) {
            LambdaLogger.Log($"*** INFO: Request input from user: {JsonConvert.SerializeObject(input)}");
            var intentRequest = (IntentRequest) input.Request;

            // process request
            return _playSongRequest.HandleRequest(intentRequest);
        }
        
        string ICommonDependencyProvider.DateNow() => new DateTime().ToUniversalTime().ToString("yy-MM-ddHH:mm:ss");
        Task<IEnumerable<SongModel.Song>> IDynamodbDependencyProvider.DynamoDbFindSongsByNumberAsync(string title) => _jukeboxDynamoDb.FindSongsByNumberAsync(title);
    }
}
