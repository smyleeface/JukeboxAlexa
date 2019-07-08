using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Castle.Core.Internal;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.PlaySongTitleRequest {
    public class PlaySongTitleRequest : AIntentRequest {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider DynamodbProvider;
        public SongModel.Song SongRequested;
        public IEnumerable<SongModel.Song> FoundSongs;

        //--- Constructor ---
        public PlaySongTitleRequest(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient, string queueUrl, IDynamodbDependencyProvider awsDynmodbProvider) : base(provider, awsSqsClient, queueUrl) {
            DynamodbProvider = awsDynmodbProvider;
        }

        //--- Methods ---
        public override async Task<CustomSkillResponse> HandleRequest(CustomSkillRequest customSkillRequest) {
            var intentSlots = customSkillRequest.Intent.Slots;
            var intentName = customSkillRequest.Intent.Name;
            FoundSongs = new List<SongModel.Song>();
            SongRequested = new SongModel.Song();
            
            // lookup song title and artist
            GetSongInfoRequested(intentSlots);
            if (IsValidRequest()) {
                await FindRequestedSong();
            }

            // if no songs found, top matches
            if (FoundSongs.ToList().Count == 0) {
                await FindSimilarSongs();
            }
                
            // generate sqs body and send to the queue
            var generatedMessage = GenerateMessage();
            if (FoundSongs.ToList().Count == 1) {
                var sqsRequest = GenerateJukeboxSqsRequest(intentName, generatedMessage, FoundSongs.ToList().FirstOrDefault().SongNumber);
                await SendSqsRequest(sqsRequest, intentName);
            } else if (FoundSongs.ToList().Count > 1) {
                generatedMessage += "Top 3 matches: ";
                generatedMessage += FoundSongs.Aggregate(" ", (current, song) => current + $"Song {song.SongNumber} {song.Title} by {song.Artist}. ");
            }

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
            if (FoundSongs.IsNullOrEmpty() || FoundSongs.ToList().Count < 0) {
                message = $"No song found for {SongRequested.Title}";
                LambdaLogger.Log($"*** WARNING: {message}");
            }

            // Handle more than one song returned. (i.e. same song title different artist.)
            if (FoundSongs.ToList().Count > 1) {
                message = $"More than one song found for {SongRequested.Title}. ";
                LambdaLogger.Log($"*** WARNING: {message}");
            }
            
            // found one song
            if (FoundSongs.ToList().Count == 1) {
                message = $"Sending song number {FoundSongs.ToList()[0].SongNumber}, {FoundSongs.ToList()[0].Title} by {FoundSongs.ToList()[0].Artist}, to the jukebox.";
                LambdaLogger.Log($"*** INFO: {message}");
            }
            
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

        public async Task FindRequestedSong() {
            var foundSongs = new List<SongModel.Song>();
            var foundDbSongs = (await DynamodbProvider.DynamoDbFindSongsByTitleAsync(SongRequested.Title)).ToList();
            if (foundDbSongs.Count < 1) {
                return;
            }
            foreach (var foundSong in foundDbSongs) {
                foundSongs.Add(foundSong);
            }
            FoundSongs = foundSongs;
        }

        public async Task FindSimilarSongs() {
            var listOfWords = SongRequested.Title.Split(" ")
                .Select(word => 
                    new Dictionary<string, AttributeValue> {
                        ["word"] = new AttributeValue { S = word.ToLower() }
                    }
                ).ToList();
            var foundDbSongs = new List<SongModel.Song>();
            foreach (var word in listOfWords) {
                var response = await DynamodbProvider.DynamoDbFindSimilarSongsAsync(word);
                Console.WriteLine($"responseresponse {JsonConvert.SerializeObject(response)}");
                if (!response.Item.IsNullOrEmpty()) {
                    foreach (var songData in response.Item["songs"].L) {
                        foundDbSongs.Add(JsonConvert.DeserializeObject<SongModel.Song>(songData.S));
                    }
                }
            }
            if (foundDbSongs.Count < 1) {
                return;
            }
            var rnd = new Random();
            var countOfDistinctSong = foundDbSongs.GroupBy(song => song.SongNumber)
                                                  .Select(x => new {
                                                      Count = x.Count(), 
                                                      Song = x.FirstOrDefault()
                                                  })
                                                  .OrderBy(x => rnd.Next())
                                                  .ToList()
                                                  .OrderByDescending(x => x.Count)
                                                  .Select(x => x.Song)
                                                  .Take(3);
            Console.WriteLine($"countOfDistinctSong {JsonConvert.SerializeObject(countOfDistinctSong)}");
            FoundSongs = countOfDistinctSong;
        }
    }
}



