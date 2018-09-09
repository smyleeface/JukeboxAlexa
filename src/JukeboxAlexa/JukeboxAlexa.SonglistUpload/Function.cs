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
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa.SonglistUpload {
    public class Function : IDynamodbDependencyProvider, IS3DependencyProvider  {
        
        //--- Fields ---
        private readonly SonglistUpload _songlistUpload;
        private readonly JukeboxDynamoDb _jukeboxDynamoDb;
        private readonly JukeboxS3 _jukeboxS3;
        
        
        //--- Constructors ---
        public Function() {
            var tableName = Environment.GetEnvironmentVariable("STR_DYNAMODBSONGS");
            var indexNameSearchTitle = Environment.GetEnvironmentVariable("INDEX_NAME_SEARCH_TITLE");
            var indexNameSearchTitleArtist = Environment.GetEnvironmentVariable("INDEX_NAME_SEARCH_TITLE_ARTIST");
            var indexTableName = Environment.GetEnvironmentVariable("STR_DYNAMODBTITLEWORDCACHE");
            _jukeboxDynamoDb = new JukeboxDynamoDb(new AmazonDynamoDBClient(), tableName, indexNameSearchTitle, indexNameSearchTitleArtist, indexTableName);
            _jukeboxS3 = new JukeboxS3(new AmazonS3Client());
            _songlistUpload = new SonglistUpload(this, this);
        }

        //--- FunctionHandler ---
        public async Task FunctionHandlerAsync(S3Event s3Event, ILambdaContext context) {
            LambdaLogger.Log($"*** INFO: PutObjectRequest: {JsonConvert.SerializeObject(s3Event)}");
            var bucketName = s3Event.Records.FirstOrDefault().S3.Bucket.Name;
            var keyName = s3Event.Records.FirstOrDefault().S3.Object.Key;
            
            // process request
            await _songlistUpload.HandleRequest(bucketName, keyName);
        }
        
        Task<IEnumerable<SongModel.Song>> IDynamodbDependencyProvider.DynamoDbFindSongsByTitleAsync(string title) => _jukeboxDynamoDb.FindSongsByTitleAsync(title);
        Task<ScanResponse> IDynamodbDependencyProvider.DynamoDbScanAsync() => _jukeboxDynamoDb.ScanAsync();
        Task<BatchWriteItemResponse> IDynamodbDependencyProvider.DynamoDbBatchWriteItemAsync(IEnumerable<WriteRequest> writeRequests) => _jukeboxDynamoDb.BatchWriteItemAsync(writeRequests);
        Task<string> IS3DependencyProvider.GetSongsFromS3UploadAsync(string bucket, string key) => GetSongsFromS3UploadAsync(bucket, key);

        public async Task<string> GetSongsFromS3UploadAsync(string bucket, string key) {
            var response = await _jukeboxS3.GetObjectAsync(bucket, key);
            var responseBody = "";
            using (StreamReader reader = new StreamReader(response.ResponseStream)) {
                responseBody = reader.ReadToEnd(); // Now you process the response body.
            }
            return responseBody;
        }
    }
}
