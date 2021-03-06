﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using JukeboxAlexa.Library;
using LambdaSharp;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa.SonglistIndex {
    public class Function : ALambdaFunction<DynamoDBEvent, string>, IDynamodbDependencyProvider  {
        
        //--- Fields ---
        private Logic _songlistUpload;
        private JukeboxDynamoDb _jukeboxDynamoDb;
        
        
        //--- Constructors ---
        public override Task InitializeAsync(LambdaConfig config) {
            var tableName = AwsConverters.ConvertDynamoDBArnToName(config.ReadText(("DynamoDbSongs")));
            var indexNameSearchTitle = config.ReadText("DynamoDbIndexNameSearchTitleName");
            var indexNameSearchTitleArtist = config.ReadText("DynamoDbIndexNameSearchTitleArtistName");
            var indexTableName = AwsConverters.ConvertDynamoDBArnToName(config.ReadText("DynamoDbTitleWordCache"));
            _jukeboxDynamoDb = new JukeboxDynamoDb(new AmazonDynamoDBClient(), tableName, indexNameSearchTitle, indexNameSearchTitleArtist, indexTableName);
            _songlistUpload = new Logic(this);
            return Task.CompletedTask;
        }

        //--- FunctionHandler ---
        public override async Task<string> ProcessMessageAsync(DynamoDBEvent dynamoDbEvent) {
            LambdaLogger.Log($"*** INFO: Event: {JsonConvert.SerializeObject(dynamoDbEvent)}");
            
            // process request
            try {
                await _songlistUpload.HandleRequest(dynamoDbEvent.Records.FirstOrDefault());
            }
            catch (Exception e) {
                LambdaLogger.Log($"Exception occured: {e}");
            }
            return "run complete";
        }
        
        Task<GetItemResponse> IDynamodbDependencyProvider.DynamodbGetItemAsync(IDictionary<string, AttributeValue> key) => _jukeboxDynamoDb.GetItemAsync(key);
        Task<UpdateItemResponse> IDynamodbDependencyProvider.DynamodbUpdateItemAsync(Dictionary<string, AttributeValue> key, string updateExpression, IDictionary<string, AttributeValue> expressionAttributeValues) => _jukeboxDynamoDb.UpdateItemAsync(key, updateExpression, expressionAttributeValues);
    }
}
