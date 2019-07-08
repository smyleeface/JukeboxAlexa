using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Amazon.SQS;
using Amazon.SQS.Model;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using JukeboxAlexa.Library.TestFixtures;
using Moq;
using Newtonsoft.Json;
using Xunit;


namespace JukeboxAlexa.PlaySongTitleRequest.Tests {
    public class PlaySongTitleRequestTest {
        public SongFixtures SongFixtures = new SongFixtures();
        
        [Fact]
        public static void Play_song_request__is_valid_request__valid() {

            // Arrange
            var tempSongFixtures = new SongFixtures();
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = tempSongFixtures.Song1;
            playSongRequest.SongRequested.Artist = "";
            playSongRequest.SongRequested.SongNumber = "";

            // Act
            var response = playSongRequest.IsValidRequest();

            // Assert
            Assert.True(response);
        }
  
        [Fact]
        public static void Play_song_request__is_valid_request__invalid() {

            // Arrange
            var tempSongFixtures = new SongFixtures();
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = tempSongFixtures.Song1;
            playSongRequest.SongRequested.Artist = "";
            playSongRequest.SongRequested.Title = "";

            // Act
            var response = playSongRequest.IsValidRequest();

            // Assert
            Assert.False(response);
        }

        [Fact]
        public void Play_song_request__generate_message__found_one_song() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;
            playSongRequest.FoundSongs = new List<SongModel.Song> {
                SongFixtures.Song1
            };

            // Act
            var response = playSongRequest.GenerateMessage();

            // Assert
            Assert.Contains("Sending song number 328", response);
        }

        [Fact]
        public void Play_song_request__generate_message__found_no_song() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;
            playSongRequest.FoundSongs = new List<SongModel.Song>();

            // Act
            var response = playSongRequest.GenerateMessage();

            // Assert
            Assert.Contains("No song found for I Will Wait", response);
        }
        
        [Fact]
        public void Play_song_request__generate_message__found_multiple_song() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;
            playSongRequest.FoundSongs = new List<SongModel.Song> {
                SongFixtures.Song2,
                SongFixtures.Song1,
                SongFixtures.Song3
            };

            // Act
            var response = playSongRequest.GenerateMessage();

            // Assert
            Assert.Contains("More than one song found for I Will Wait", response);
        }
        
        [Fact]
        public static void Play_song_request__get_song_info_requested__found_all_slots() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            var intentSlots = new Dictionary<string, Slot> {
                { "Title", new Slot {
                        Name   = "Title",
                        Value = "I Will Wait"
                    }
                }

            };

            // Act
            playSongRequest.GetSongInfoRequested(intentSlots);

            // Assert
            Assert.Equal("I Will Wait", playSongRequest.SongRequested.Title);
            Assert.Null(playSongRequest.SongRequested.Artist);
            Assert.Null(playSongRequest.SongRequested.SongNumber);
        }

        [Fact]
        public static void Play_song_request__get_song_info_requested__found_no_slots() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            var intentSlots = new Dictionary<string, Slot> {
                { "Title", new Slot {
                        Name   = "Title",
                        Value = "I Will Wait"
                    }
                }

            };

            // Act
            playSongRequest.GetSongInfoRequested(intentSlots);

            // Assert
            Assert.Equal("I Will Wait", playSongRequest.SongRequested.Title);
            Assert.Null(playSongRequest.SongRequested.Artist);
            Assert.Null(playSongRequest.SongRequested.SongNumber);
        }
        
        [Fact]
        public async void Play_song_request__find_requested_song__found_one() {

            // Arrange
            IEnumerable<SongModel.Song> foundDynamodbSongs = new List<SongModel.Song> {
                SongFixtures.Song1
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByTitleAsync("I Will Wait")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;


            // Act
            await playSongRequest.FindRequestedSong();

            // Assert
            Assert.Equal("I Will Wait", playSongRequest.FoundSongs.ToList().FirstOrDefault().Title);
            Assert.Equal("Mumford & Sons", playSongRequest.FoundSongs.ToList().FirstOrDefault().Artist);
            Assert.Equal("328", playSongRequest.FoundSongs.ToList().FirstOrDefault().SongNumber);
        }
        
        [Fact]
        public async void Play_song_request__find_requested_song__found_multiple() {

            // Arrange
            IEnumerable<SongModel.Song> foundDynamodbSongs = new List<SongModel.Song> {
                SongFixtures.Song1,
                SongFixtures.Song2,
                SongFixtures.Song3
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByTitleAsync("I Will Wait")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object) {
                SongRequested = SongFixtures.Song1
            };


            // Act
            await playSongRequest.FindRequestedSong();

            // Assert
            Assert.Equal("I Will Wait", playSongRequest.FoundSongs.ToList().FirstOrDefault().Title);
            Assert.Equal("Mumford & Sons", playSongRequest.FoundSongs.ToList().FirstOrDefault().Artist);
            Assert.Equal("328", playSongRequest.FoundSongs.ToList().FirstOrDefault().SongNumber);
        }
        
        [Fact]
        public async Task Play_song_request__handle_request() {
            
            // Arrange
            var customSkillRequest = new CustomSkillRequest {
                Intent = new Intent {
                    Name = "PlaySongTitleRequest",
                    Slots = new Dictionary<string, Slot> {
                        { 
                            "Title", new Slot {
                                Name   = "Title",
                                Value = "I Will Wait"
                            }
                        }
                    }
                },
                DialogState = "STARTED",
                Type = "PlaySongTitleRequest"
            };
            
            // mock dependency provider common
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            
            // mock dependency provider sqs
            var request = new JukeboxSqsRequest {
                Key = "328",
                MessageBody = "foo-bar",
                RequestType = "PlaySongTitleArtistRequest"
            };
            var sendMessageRequest = new SendMessageRequest {
                QueueUrl = "http://foo-bar",
                MessageGroupId = "bat-baz",
                MessageDeduplicationId = "foo-date",
                MessageBody = JsonConvert.SerializeObject(request)
            };
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            sqsClient.Setup(x => x.SendMessageAsync(sendMessageRequest, new CancellationToken()));
            
            // mock dependency provider dynamodb
            IEnumerable<SongModel.Song> foundDynamodbSongs = new List<SongModel.Song> {
                SongFixtures.Song1
            };
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByTitleAsync("I Will Wait")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongRequest = new PlaySongTitleRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            
            // Act
            var response = await playSongRequest.HandleRequest(customSkillRequest);
            
            // Assert
            Assert.Contains("Sending song number 328", response.Message); 
        }

        [Fact]
        public async Task Play_song_request__sort() {
            var foundDbSongs = new List<SongModel.Song> {
                new SongModel.Song {
                    Artist = "",
                    Title = "What Doesn't Kill You (Stronger)",
                    SongNumber = "109"
                },
                new SongModel.Song {
                    Artist = "",
                    Title = "Don't Want to Lose You Now",
                    SongNumber = "225"
                },
                new SongModel.Song {
                    Artist = "",
                    Title = "You Can't Run From Love",
                    SongNumber = "344"
                },
                new SongModel.Song {
                    Artist = "",
                    Title = "What Doesn't Kill You (Stronger)",
                    SongNumber = "109"
                },
                new SongModel.Song {
                    Artist = "",
                    Title = "adfdfad",
                    SongNumber = "100"
                }
            };
            var rnd = new Random();
            var shuffled = foundDbSongs.GroupBy(song => song.SongNumber)
                        .Select(x => new {
                            Count = x.Count(), 
                            Song = x.FirstOrDefault()
                        })
                        .OrderBy(x => rnd.Next()).ToList();
            Console.Write("hello");
        }
    }
}