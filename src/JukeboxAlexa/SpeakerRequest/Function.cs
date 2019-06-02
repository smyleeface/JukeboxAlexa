using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
namespace JukeboxAlexa.SpeakerRequest {
    public class Function : ALambdaApiGatewayFunction, ICommonDependencyProvider {
        
        //--- Fields ---
        public SpeakerRequest SpeakerRequest;
        
        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config) {
            var sqsClient = new AmazonSQSClient();
            var queueName = Environment.GetEnvironmentVariable("STR_SQSSONGQUEUE");
            SpeakerRequest = new SpeakerRequest(this, sqsClient, queueName);
            return Task.CompletedTask;
        }
        
        public override async Task<APIGatewayProxyResponse> ProcessProxyRequestAsync(APIGatewayProxyRequest inputRequest) {
            LambdaLogger.Log($"*** INFO: API Request input from user: {JsonConvert.SerializeObject(inputRequest)}");
            var input = JsonConvert.DeserializeObject<CustomSkillRequest>(inputRequest.Body);
            LambdaLogger.Log($"*** INFO: Request input from user: {input}");
    
            // process request
            var requestResult = await SpeakerRequest.HandleRequest(input);
            var response = new APIGatewayProxyResponse {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(requestResult),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" }
                }
            };
            return response;    
        }
        
        // --- Dependecy Providers ---
        string ICommonDependencyProvider.DateNow() => new DateTime().ToUniversalTime().ToString("yy-MM-ddHH:mm:ss");
    }
}
