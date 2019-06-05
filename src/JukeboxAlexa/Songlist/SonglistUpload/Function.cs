using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using LambdaSharp;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa.SonglistUpload {
    public class Function : ALambdaFunction<S3Event, string>, IDynamodbDependencyProvider, IS3DependencyProvider  {
        
        //--- Fields ---
        public Logic SonglistUpload;
        public JukeboxDynamoDb JukeboxDynamoDb;
        public JukeboxS3 JukeboxS3;

        //--- Constructors ---
        public override Task InitializeAsync(LambdaConfig config) {
            var tableName = AwsConverters.ConvertDynamoDBArnToName(config.ReadText(("DynamoDbSongs")));
            var indexNameSearchTitle = config.ReadText("DynamoDbIndexNameSearchTitleName");
            var indexNameSearchTitleArtist = config.ReadText("DynamoDbIndexNameSearchTitleArtistName");
            var indexTableName = AwsConverters.ConvertDynamoDBArnToName(config.ReadText("DynamoDbTitleWordCache"));
            JukeboxDynamoDb = new JukeboxDynamoDb(new AmazonDynamoDBClient(), tableName, indexNameSearchTitle, indexNameSearchTitleArtist, indexTableName);
            JukeboxS3 = new JukeboxS3(new AmazonS3Client());
            SonglistUpload = new Logic(this, this);
            
            return Task.CompletedTask;
        }

        //--- FunctionHandler ---
        public override async Task<string> ProcessMessageAsync(S3Event s3Event) {
            LambdaLogger.Log($"*** INFO: PutObjectRequest: {JsonConvert.SerializeObject(s3Event)}");
            var bucketName = s3Event.Records.FirstOrDefault().S3.Bucket.Name;
            var keyName = s3Event.Records.FirstOrDefault().S3.Object.Key;
            
            // process request
            await SonglistUpload.HandleRequest(bucketName, keyName);
            return "upload complete";
        }
        
        Task<IEnumerable<SongModel.Song>> IDynamodbDependencyProvider.DynamoDbFindSongsByTitleAsync(string title) => JukeboxDynamoDb.FindSongsByTitleAsync(title);
        Task<ScanResponse> IDynamodbDependencyProvider.DynamoDbScanAsync() => JukeboxDynamoDb.ScanAsync();
        Task<BatchWriteItemResponse> IDynamodbDependencyProvider.DynamoDbBatchWriteItemAsync(IEnumerable<WriteRequest> writeRequests) => JukeboxDynamoDb.BatchWriteItemAsync(writeRequests);
        Task<GetObjectResponse> IS3DependencyProvider.S3GetObjectAsync(string bucket, string key) => JukeboxS3.GetObjectAsync(bucket, key);
        string IS3DependencyProvider.ReadS3Stream(Stream stream) => JukeboxS3.ReadS3Stream(stream);
    }
}
