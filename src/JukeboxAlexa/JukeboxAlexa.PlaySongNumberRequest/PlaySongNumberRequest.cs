using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Castle.Core.Internal;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.PlaySongNumberRequest {
    public class PlaySongNumberRequest : AIntentRequest {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider DynamodbProvider;
        public SongModel.Song SongRequested;
        public IEnumerable<SongModel.Song> FoundSongs;

        //--- Constructor ---
        public PlaySongNumberRequest(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient, string queueUrl, IDynamodbDependencyProvider awsDynmodbProvider) : base(provider, awsSqsClient, queueUrl) {
            SongRequested = new SongModel.Song();
            FoundSongs = new List<SongModel.Song>();
            DynamodbProvider = awsDynmodbProvider;
        }

        //--- Methods ---
        public override async Task<CustomSkillResponse> HandleRequest(CustomSkillRequest customSkillRequest) {
            var intentSlots = customSkillRequest.Intent.Slots;
            var intentName = customSkillRequest.Intent.Name;
            
            // lookup song title and artist
            GetSongInfoRequested(intentSlots);
            if (IsValidRequest()) {
                FindRequestedSong();
            }

            // generate sqs body and send to the queue
            var generatedMessage = GenerateMessage();
            var sqsReuqest = GenerateJukeboxSqsRequest(intentName, generatedMessage, "");
            await SendSqsRequest(sqsReuqest, intentName);

            // generate alexa response
            var customSkillResponse = new CustomSkillResponse {
                Message = generatedMessage
            };
            LambdaLogger.Log($"*** INFO: Alexa response to user: {JsonConvert.SerializeObject(customSkillResponse)}");
            return customSkillResponse;
        }
        
        public override bool IsValidRequest() {
            var isValid = !SongRequested.SongNumber.IsNullOrEmpty();
            LambdaLogger.Log(isValid ? "*** INFO: Valid request" : "*** INFO: Invalid request");
            return isValid;
        }

        public override string GenerateMessage() {
            var message = "Sorry I do not understand.";

            // Handle no song returned.
            if (FoundSongs.IsNullOrEmpty()) {
                message = $"No song found for {SongRequested.SongNumber}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }

            // Handle more than one song returned. (i.e. same song title different artist.)
            if (FoundSongs.ToList().Count > 1) {
                message = $"More than one song found for {SongRequested.SongNumber}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }
            if (FoundSongs.ToList().Count != 1) {
                return message;
            }
            
            // found one song
            message = $"Sending song number {FoundSongs.ToList().FirstOrDefault().SongNumber}, {FoundSongs.ToList()[0].Title} by {FoundSongs.ToList()[0].Artist}, to the jukebox.";
            LambdaLogger.Log($"*** INFO: {message}");

            return message;
        }

        public override void GetSongInfoRequested(Dictionary<string, Slot> intentSlots) {
            var trackFound = intentSlots.TryGetValue("SongNumber", out Slot trackRequested);
            if (trackFound) {
                SongRequested.SongNumber = trackRequested.Value;
                LambdaLogger.Log($"*** INFO: SongNumber {trackRequested.Value}");
            }
        }

        public async Task FindRequestedSong() {
            var foundSongs = new List<SongModel.Song>();
            var foundDbSongs = (await DynamodbProvider.DynamoDbFindSongsByNumberAsync(SongRequested.SongNumber)).ToList();
            LambdaLogger.Log($"*** INFO: Dynamodb Response: {JsonConvert.SerializeObject(foundDbSongs)}");
            if (foundDbSongs.Count < 1) {
                return;
            }
            foreach (var foundSong in foundDbSongs) {
                foundSongs.Add(foundSong);
            }
            FoundSongs = foundSongs;
        }
    }
}



