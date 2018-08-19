using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library {
    public class JukeboxS3 {
        
        //--- Fields ---
        private IAmazonS3 _s3Client;

        //--- Constructors ---
        public JukeboxS3(IAmazonS3 s3Client) {
            _s3Client = s3Client;
        }
        
        //--- Methods ---
        public async Task<GetObjectResponse> GetObjectAsync(string bucketName, string keyName) {
            return await _s3Client.GetObjectAsync(new GetObjectRequest {
                BucketName = bucketName,
                Key = keyName
            });
        }
    }
}