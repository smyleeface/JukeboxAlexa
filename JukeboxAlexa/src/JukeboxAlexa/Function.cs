using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Ssml;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JukeboxAlexa.IntentLogic;
using JukeboxAlexa.Library;
using JukeboxAlexa.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace JukeboxAlexa
{
    public class Function
    {
        
        //--- Fields ---
        private static string _queueUrl;
        private static IAmazonSQS _sqsClient;

        //--- Constructors ---
        public Function()
        {
            _sqsClient = new AmazonSQSClient();
            _queueUrl = "";
        }

        //--- FunctionHandler ---
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            Console.WriteLine($"*** INFO: Request input -> {JsonConvert.SerializeObject(input)}");
            
            // initialize variables
            var songLookup = new LookupResultModel.SongLookup();
            var intentRequest = (IntentRequest)input.Request;
            
            // process intent request if of valid type
            if (input.GetRequestType() == typeof(IntentRequest))
            {
                
                // get the song title
//                if (!intentRequest.Intent.Slots.TryGetValue("Title", out Slot titleRequested))
//                {
//                    var intentsThatNeedTitleString = "PlaySongRequest, PlaySongArtistRequest, FindSongRequested";
//                    //var intentsThatNeedTitle = intentsThatNeedTitleString.Split(',');
//                    if (intentRequest.Intent.Name.Contains(intentsThatNeedTitleString))
//                    Console.WriteLine($"*** ERROR: Cannot find Title in Intent Slot - {JsonConvert.SerializeObject(intentRequest)}");
//                    songLookup.SpeechText = $"Cannot understand request.";
//                }
                intentRequest.Intent.Slots.TryGetValue("Title", out Slot titleRequested);
                Console.WriteLine($"*** INFO: Title {titleRequested.Value}");
                
                // get the artist
                intentRequest.Intent.Slots.TryGetValue("ArtistName", out Slot artistRequested);
                Console.WriteLine($"*** INFO: ArtistName {artistRequested}");
                //TODO required intent check

                // get the track number
                intentRequest.Intent.Slots.TryGetValue("TrackNumber", out Slot numberRequested);
                Console.WriteLine($"*** INFO: TrackNumber {numberRequested}");
                //TODO required intent check
                
                songLookup.IntentType = intentRequest.Intent.Name;
                switch (songLookup.IntentType)
                {
                    case "PlaySongRequest":
                        Console.WriteLine($"**** INFO **** Intent: PlaySongRequest");
                        songLookup.SnsResponse = new SnsMessageBody.Response
                        {
                            RequestType = "PlaySongRequest"
                        };
                        var playSongRequest = new PlaySongRequest();
                        songLookup = playSongRequest.ProcessRequest(titleRequested.Value);
                        SendSongToJukeboxQueue(songLookup.SnsResponse, "PlaySongRequest");
                        return JukeboxAlexaResponse.GenerateResponseCard(songLookup.SpeechText);
                        
                    case "PlaySongNumberRequest":
                        Console.WriteLine($"**** INFO **** Intent: PlaySongNumberRequest");
                        songLookup.SnsResponse = new SnsMessageBody.Response
                        {
                            RequestType = "PlaySongNumberRequest"
                        };
                        var playSongNumberRequest = new PlaySongNumberRequest();
                        songLookup = playSongNumberRequest.ProcessRequest(numberRequested?.Value);
                        SendSongToJukeboxQueue(songLookup.SnsResponse, "PlaySongNumberRequest");
                        return JukeboxAlexaResponse.GenerateResponseCard(songLookup.SpeechText);

                    case "PlaySongArtistRequest":
                        Console.WriteLine($"**** INFO **** Intent: PlaySongArtistRequest");
                        songLookup.SnsResponse = new SnsMessageBody.Response
                        {
                            RequestType = "PlaySongArtistRequest"
                        };
                        var playSongArtistRequest = new PlaySongArtistRequest();
                        songLookup = playSongArtistRequest.ProcessRequest(titleRequested?.Value, artistRequested?.Value);
                        SendSongToJukeboxQueue(songLookup.SnsResponse, "PlaySongArtistRequest");
                        return JukeboxAlexaResponse.GenerateResponseCard(songLookup.SpeechText);
                        
                    case "FindSongRequested":

                        if (!string.IsNullOrEmpty(intentRequest.DialogState) && intentRequest.DialogState != "STARTED")
                        {
                            //var finalResponse = ResponseBuilder.Ask(songLookup.SpeechText, songLookup.RepromptBody);
                            Console.WriteLine($"*** IN PROGRESS - {JsonConvert.SerializeObject(intentRequest)}");
                            LookupResultModel.SongLookup songLookup2 = new LookupResultModel.SongLookup
                            {
                                Request = new SongModel.Song
                                {
                                    Title = "test test"
                                },
                                Response = new List<SongModel.Song>()
                            };
                            songLookup2.Response.Add(new SongModel.Song {
                                Title = "test test",
                                Artist = "testa",
                                TrackNumber = "111"
                            });
                            songLookup2.SpeechText = "test test speech text";
                            return JukeboxAlexaResponse.GenerateResponseCard(songLookup2.SpeechText);
                        }
                        
                        // get the song title from the request
                        if (!intentRequest.Intent.Slots.TryGetValue("Title", out Slot phraseRequested))
                        {
                            Console.WriteLine(
                                $"*** ERROR: Cannot find Title in Intent Slot - {JsonConvert.SerializeObject(intentRequest)}");
                            songLookup.SpeechText = $"Cannot understand request.";
                            return ResponseBuilder.Tell(songLookup.SpeechText);
                        }

                        var findSongRequested = new FindSongRequested();
                        songLookup = findSongRequested.ProcessRequest(phraseRequested?.Value);
                        

                        // create the response
                        var finalResponse = ResponseBuilder.DialogDelegate(intentRequest.Intent);
                        finalResponse.Response = new ResponseBody
                        {
                            OutputSpeech = new PlainTextOutputSpeech {
                                Text = songLookup.SpeechText
                            }
                        };
                        //var finalResponse = ResponseBuilder.Ask(songLookup.SpeechText, songLookup.RepromptBody);
                        //SendSongToJukeboxQueue(songLookup.Response[0].TrackNumber, "FindSongRequested");
                        return finalResponse;
                }
            }
           
            return JukeboxAlexaResponse.GenerateResponseCard("Unknown Intent Request");

        }

        private void SendSongToJukeboxQueue(SnsMessageBody.Response snsMessageBodyResponse, string requestType)
        {
            _queueUrl = GetQueueUrl();
            _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageGroupId = requestType,
                MessageDeduplicationId = new DateTime().ToUniversalTime().ToString("yy-MM-ddHH:mm:ss"),
                MessageBody = JsonConvert.SerializeObject(snsMessageBodyResponse)
            });
        }

        protected string GetQueueUrl()
        {
            var listJukeboxQueue = _sqsClient.ListQueuesAsync(
                new ListQueuesRequest
                {
                    QueueNamePrefix = "jukebox_request_queue.fifo" 
                }
            ).Result;
            LambdaLogger.Log(JsonConvert.SerializeObject(listJukeboxQueue));
            return listJukeboxQueue.QueueUrls[0];
        }
    }
}
