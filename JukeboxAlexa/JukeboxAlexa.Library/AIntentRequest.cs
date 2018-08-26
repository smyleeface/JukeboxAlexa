using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Castle.Core.Internal;
using JukeboxAlexa.Library.Model;
using JukeboxAlexa.Library;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library {
    public abstract class AIntentRequest {
        
        // ----- Fields -----
        private string _queueUrl;
        public readonly IAmazonSQS sqsClient;
        private readonly ICommonDependencyProvider _provider;

        public AIntentRequest(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient, string queueUrl) {
            _provider = provider;
            sqsClient = awsSqsClient;
            _queueUrl = queueUrl;
        }
        
        public abstract Task<CustomSkillResponse> HandleRequest(CustomSkillRequest customSkillRequest);
        public abstract bool IsValidRequest();
        public abstract string GenerateMessage();
        public abstract void GetSongInfoRequested(Dictionary<string, Slot> intentSlots);

        public virtual JukeboxSqsRequest GenerateJukeboxSqsRequest(string reqeustType, string message, string key) {
            if (!IsValidRequest()) {
                return null;
            }
            return new JukeboxSqsRequest {
                RequestType = reqeustType,
                Key = key,
                MessageBody = message
            };
        }

        public virtual async Task<SendMessageResponse> SendSqsRequest(JukeboxSqsRequest request, string requestType) {
            try {

                if (request == null || _queueUrl.IsNullOrEmpty()) return null;
                var sendMessageRequest = new SendMessageRequest {
                    QueueUrl = _queueUrl,
                    MessageGroupId = requestType,
                    MessageDeduplicationId = _provider.DateNow(),
                    MessageBody = JsonConvert.SerializeObject(request)
                };
                LambdaLogger.Log($"Sending Sqs Message to Jukebox: {JsonConvert.SerializeObject(sendMessageRequest)}");
                var sendMessageResponse = await sqsClient.SendMessageAsync(sendMessageRequest);
                return sendMessageResponse;
            }
            catch (Exception e) {
                LambdaLogger.Log($"There was a problem sending message: {e}");
            }

            return null;
        }
        

    }
}