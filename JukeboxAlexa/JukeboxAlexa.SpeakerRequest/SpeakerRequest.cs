using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.SQS;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.SpeakerRequest {
    public class SpeakerRequest : AIntentRequest {
        
        //--- Fields ---
        public string SpeakerAction;
        
        //--- Constructor ---
        public SpeakerRequest(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient, string queueName) : base(provider, awsSqsClient, queueName) {}

        //--- Methods ---
        public override async Task<CustomSkillResponse> HandleRequest(CustomSkillRequest intentRequest) {

            var intentSlots = intentRequest.Intent.Slots;
            var intentName = intentRequest.Intent.Name;
            
            GetSongInfoRequested(intentSlots);

            // generate sqs body and send to the queue
            var generatedMessage = GenerateMessage();
            var sqsReuqest = GenerateJukeboxSqsRequest(intentName, generatedMessage, SpeakerAction);
            await SendSqsRequest(sqsReuqest, intentName);

            // generate alexa response
            var customSkillResponse = new CustomSkillResponse {
                Message = generatedMessage
            };
            LambdaLogger.Log($"*** INFO: Alexa response to user: {JsonConvert.SerializeObject(customSkillResponse)}");
            return customSkillResponse;
        }

        public override bool IsValidRequest() {
            var isValid = SpeakerAction == "on" || SpeakerAction == "off";;
            LambdaLogger.Log(isValid ? "*** INFO: Valid request" : "*** INFO: Invalid request");
            return isValid;
        }

        public override void GetSongInfoRequested(Dictionary<string, Slot> intentSlots) {
            var options = intentSlots.TryGetValue("Options", out Slot speakerOption);
            if (!options) return;
            SpeakerAction = speakerOption.Value;
            LambdaLogger.Log($"*** INFO: Speaker option {speakerOption.Value}");
        }
        
        public override string GenerateMessage() {
            var validMessage = $"Turning the jukebox speaker {SpeakerAction}";
            var invalidMessage = $"I do not understand speaker request `{SpeakerAction}`";
            return !IsValidRequest() ? invalidMessage : validMessage;
        }
    }
}