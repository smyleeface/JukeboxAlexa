using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Castle.Core.Internal;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa.SkillProxyRequest
{
    public class Function {
        
        //--- Fields ---
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        //--- Constructors ---
        public Function() {
            _httpClient = new HttpClient(); 
            _endpoint = Environment.GetEnvironmentVariable("API_ENDPOINT");
        }

        //--- FunctionHandler ---
        public async Task<SkillResponse> FunctionHandlerAsync(SkillRequest input, ILambdaContext context) {
            
            LambdaLogger.Log($"*** INFO: Request input from user: {JsonConvert.SerializeObject(input)}");
            
            var intentRequest = (IntentRequest) input.Request;
            var intentName = intentRequest.Intent.Name;
            var finalResponse = ResponseBuilder.Tell("Sorry I do not understand");
            var endpointPath = "";
            
            // create custom skill request
            var customSkillRequest = new CustomSkillRequest {
                DialogState = intentRequest.DialogState,
                Intent = intentRequest.Intent,
                Type = intentRequest.Type
            };
            LambdaLogger.Log($"**** INFO **** customSkillRequest: {customSkillRequest}");
            
            switch (intentName) {
                
                //
                //--------------------------
                // SPEAKER REQUEST
                //--------------------------
                //
                case "SpeakerRequest": {
                    LambdaLogger.Log("**** INFO **** Intent: SpeakerRequest");
                    endpointPath = $"{_endpoint}/jukebox-alexa/speaker-request";
                }
                break;
                
                //
                //--------------------------
                // PLAY SONG TITLE REQUEST
                //--------------------------
                //
                case "PlaySongTitleRequest": {
                    LambdaLogger.Log("**** INFO **** Intent: PlaySongTitleRequest");
                    endpointPath = $"{_endpoint}/jukebox-alexa/song-title-request";
                }
                break;
                
//
                //--------------------------
                // PLAY NUMBER REQUEST
                //--------------------------
                //
                case "PlaySongNumberRequest": {
                    LambdaLogger.Log("**** INFO **** Intent: PlayNumberRequest");
                    endpointPath = $"{_endpoint}/jukebox-alexa/song-number-request";
                }
                    break;
                
//
                //--------------------------
                // PLAY SONG TITLE ARTIST REQUEST
                //--------------------------
                //
                case "PlaySongTitleArtistRequest": {
                    LambdaLogger.Log("**** INFO **** Intent: PlaySongTitleArtistRequest");
                    endpointPath = $"{_endpoint}/jukebox-alexa/song-title-artist-request";
                }
                    break;
                
                //
                //--------------------------
                // DEFAULT
                //--------------------------
                //
                default:
                    LambdaLogger.Log($"**** INFO **** Intent: Unsupported");
                    break;
            }

            if (!endpointPath.IsNullOrEmpty()) {
                LambdaLogger.Log($"*** INFO: endpointPath: {endpointPath}");
                var endpointPayload = new StringContent(JsonConvert.SerializeObject(customSkillRequest));
                LambdaLogger.Log($"*** INFO: endpointPayload: {await endpointPayload.ReadAsStringAsync()}");
                var endpointResponse = await _httpClient.PostAsync(endpointPath, endpointPayload);
                LambdaLogger.Log($"*** INFO: endpointResponse: {JsonConvert.SerializeObject(endpointResponse)}");
                var responseResult = await endpointResponse.Content.ReadAsStringAsync();
                var customSkillResponse = JsonConvert.DeserializeObject<CustomSkillResponse>(responseResult);
                LambdaLogger.Log($"*** INFO: customSkillResponse: {JsonConvert.SerializeObject(customSkillResponse)}");
                finalResponse = AlexaResponse.Generate("tell", customSkillResponse.Message);
            }
            LambdaLogger.Log($"*** INFO: Alexa response to user: {JsonConvert.SerializeObject(finalResponse)}");
            return finalResponse;
        }
    }
}
