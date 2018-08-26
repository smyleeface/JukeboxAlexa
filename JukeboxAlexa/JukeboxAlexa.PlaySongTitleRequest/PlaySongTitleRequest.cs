using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Castle.Core.Internal;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.PlaySongTitleRequest {
    public class PlaySongTitleRequest : AIntentRequest {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider dynamodbProvider;
        public readonly ICommonDependencyProvider CommonProvider;
        public SongModel.Song SongRequested;
        public IEnumerable<SongModel.Song> FoundSongs;

        //--- Constructor ---
        public PlaySongTitleRequest(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient, string queueUrl, IDynamodbDependencyProvider awsDynmodbProvider) : base(provider, awsSqsClient, queueUrl) {
            SongRequested = new SongModel.Song();
            FoundSongs = new List<SongModel.Song>();
            CommonProvider = provider;
            dynamodbProvider = awsDynmodbProvider;
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
            var sqsReuqest = GenerateJukeboxSqsRequest(intentName, generatedMessage, "hello");
            await SendSqsRequest(sqsReuqest, intentName);

            // generate alexa response
            var customSkillResponse = new CustomSkillResponse {
                Message = generatedMessage
            };
            LambdaLogger.Log($"*** INFO: Alexa response to user: {JsonConvert.SerializeObject(customSkillResponse)}");
            return customSkillResponse;
        }
        
        public override bool IsValidRequest() {
            return !SongRequested.Title.IsNullOrEmpty();
        }

        public override string GenerateMessage() {
            var message = "Sorry I do not understand.";

            // Handle no song returned.
            if (FoundSongs.IsNullOrEmpty()) {
                message = $"No song found for {SongRequested.Title}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }

            // Handle more than one song returned. (i.e. same song title different artist.)
            if (FoundSongs.ToList().Count > 1) {
                message = $"More than one song found for {SongRequested.Title}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }
            if (FoundSongs.ToList().Count != 1) return message;
            
            // found one song
            message = $"Sending song number {FoundSongs.ToList().FirstOrDefault().SongNumber}, {FoundSongs.ToList()[0].Title} by {FoundSongs.ToList()[0].Artist}, to the jukebox.";
            LambdaLogger.Log($"*** INFO: {message}");

            return message;
        }

        public override void GetSongInfoRequested(Dictionary<string, Slot> intentSlots) {
            
            // get the song name
            var titleFound = intentSlots.TryGetValue("Title", out Slot titleRequested);
            if (titleFound) {
                SongRequested.Title = titleRequested.Value;
                LambdaLogger.Log($"*** INFO: Title {titleRequested.Value}");
            }
        }

        public async void FindRequestedSong() {
            var foundSongs = new List<SongModel.Song>();
            var foundDbSongs = (await dynamodbProvider.DynamoDbFindSongsByTitleAsync(SongRequested.Title)).ToList();
            if (foundDbSongs.Count < 1) return;
            foreach (var foundSong in foundDbSongs) {
                foundSongs.Add(foundSong);
            }
            FoundSongs = foundSongs;
        }
    }
}



