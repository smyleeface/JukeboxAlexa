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

namespace JukeboxAlexa.PlaySongTitleArtistRequest {
    public class PlaySongTitleArtistRequest : AIntentRequest {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider dynamodbProvider;
        public readonly ICommonDependencyProvider CommonProvider;
        public SongModel.Song SongRequested;
        public IEnumerable<SongModel.Song> FoundSongs;

        //--- Constructor ---
        public PlaySongTitleArtistRequest(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient, string queueUrl, IDynamodbDependencyProvider awsDynmodbProvider) : base(provider, awsSqsClient, queueUrl) {
            SongRequested = new SongModel.Song();
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
            return !SongRequested.Title.IsNullOrEmpty() && !SongRequested.Artist.IsNullOrEmpty();
        }

        public override string GenerateMessage() {
            var message = "Sorry I do not understand.";

            // Handle no song returned.
            if (FoundSongs.IsNullOrEmpty()) {
                message = $"No song found for {SongRequested.Title} by {SongRequested.Artist}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }

            // Handle more than one song returned. (i.e. same song title different artist.)
            if (!FoundSongs.IsNullOrEmpty() && FoundSongs.ToList().Count > 1) {

                // TODO List artists and list in speech text
                message = $"More than one song found for {SongRequested.Title} by {SongRequested.Artist}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }
            if (FoundSongs.IsNullOrEmpty() || FoundSongs.ToList().Count != 1) return message;
            
            // found one song
            message = $"Sending song number {FoundSongs.ToList().FirstOrDefault().Number}, {FoundSongs.ToList()[0].Title} by {FoundSongs.ToList()[0].Artist}, to the jukebox.";
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
                    
            // get the artist
            var artistFound = intentSlots.TryGetValue("ArtistName", out Slot artistRequested);
            if (artistFound) {
                SongRequested.Artist = artistRequested.Value;
                LambdaLogger.Log($"*** INFO: ArtistName {artistRequested.Value}");
            }
        }

        public async void FindRequestedSong() {
            var foundSongs = new List<SongModel.Song>();
            var foundDbSongs = (await dynamodbProvider.DynamoDbFindSongsByTitleArtistAsync(SongRequested.Title, SongRequested.Artist)).ToList();
            foreach (var foundSong in foundDbSongs) {
                foundSongs.Add(foundSong);
            }
            FoundSongs = foundSongs;
        }
    }
}



