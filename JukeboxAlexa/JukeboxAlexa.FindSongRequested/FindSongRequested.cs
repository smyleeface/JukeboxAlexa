using System.Collections.Generic;
using System.Linq;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Castle.Core.Internal;
using JukeboxAlexa.Library.Model;
using JukeboxAlexa.Library;
using Newtonsoft.Json;

namespace JukeboxAlexa.FindSongRequested {
    public class FindSongRequested : AIntentRequest {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider dynamodbProvider;
        public readonly ICommonDependencyProvider CommonProvider;
        public SongModel.Song SongRequested;
        public IEnumerable<SongModel.Song> FoundSongs;

        //--- Constructor ---
        public FindSongRequested(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient, IDynamodbDependencyProvider awsDynmodbProvider) : base(provider, awsSqsClient) {
            SongRequested = new SongModel.Song();
            FoundSongs = new List<SongModel.Song>();
            CommonProvider = provider;
            dynamodbProvider = awsDynmodbProvider;
        }

        //--- Methods ---
        public override SkillResponse HandleRequest(IntentRequest intentRequest) {
            var intentSlots = intentRequest.Intent.Slots;
            var intentName = intentRequest.Intent.Name;
            
            // lookup song title and artist
            GetSongInfoRequested(intentSlots);
            if (IsValidRequest()) {
                FindRequestedSong();
            }

            // generate sqs body and send to the queue
            var generatedMessage = GenerateMessage();
            var sqsReuqest = GenerateJukeboxSqsRequest(intentName, generatedMessage, "hello");
            SendSqsRequest(sqsReuqest, intentName);

            // generate alexa response
            var finalResponse = GenerateAlexaResponse("tell", generatedMessage);
            
            // build the speech response 
            var speech = new SsmlOutputSpeech {
                Ssml = generatedMessage
            };
            var reprompt = new Reprompt {
                OutputSpeech = speech
            };
            LambdaLogger.Log($"*** INFO: Alexa response to user: {JsonConvert.SerializeObject(finalResponse)}");
            return finalResponse;
        }
        
        public override bool IsValidRequest() {
            return !SongRequested.Number.IsNullOrEmpty();
        }

        public override string GenerateMessage() {
            var message = "Sorry I do not understand.";

            // Handle no song returned.
            if (FoundSongs.IsNullOrEmpty()) {
                message = $"No song found for {SongRequested.Number}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }

            // Handle more than one song returned. (i.e. same song title different artist.)
            if (FoundSongs.ToList().Count > 1) {
                message = $"More than one song found for {SongRequested.Number}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }
            if (FoundSongs.ToList().Count != 1) return message;
            
            // found one song
            message = $"Sending song number {FoundSongs.ToList().FirstOrDefault().Number}, {FoundSongs.ToList()[0].Title} by {FoundSongs.ToList()[0].Artist}, to the jukebox.";
            LambdaLogger.Log($"*** INFO: {message}");

            return message;
        }

        public override void GetSongInfoRequested(Dictionary<string, Slot> intentSlots) {
            
            // get the song name
            var trackFound = intentSlots.TryGetValue("TrackNumber", out Slot trackRequested);
            if (trackFound) {
                SongRequested.Number = trackRequested.Value;
                LambdaLogger.Log($"*** INFO: TrackNumber {trackRequested.Value}");
            }
        }

        public async void FindRequestedSong() {
            var foundSongs = new List<SongModel.Song>();
            var foundDbSongs = (await dynamodbProvider.DynamoDbFindSongsByNumberAsync(SongRequested.Number)).ToList();
            if (foundDbSongs.Count < 1) return;
            foreach (var foundSong in foundDbSongs) {
                foundSongs.Add(foundSong);
            }
            FoundSongs = foundSongs;
        }
    }
}



