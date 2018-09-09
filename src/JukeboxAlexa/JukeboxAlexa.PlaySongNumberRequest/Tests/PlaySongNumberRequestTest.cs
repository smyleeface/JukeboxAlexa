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

namespace JukeboxAlexa.PlaySongNumberRequest.Tests {
    public class PlaySongNumberRequestTest {
        public SongFixtures SongFixtures = new SongFixtures();
        
        [Fact]
        public static void Play_song_request__is_valid_request__valid() {

            // Arrange
            var tempSongFixtures = new SongFixtures();
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = tempSongFixtures.Song1;
            playSongRequest.SongRequested.Artist = "";
            playSongRequest.SongRequested.Title = "";

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
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = tempSongFixtures.Song1;
            playSongRequest.SongRequested.Artist = "";
            playSongRequest.SongRequested.SongNumber = "";

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
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
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
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;
            playSongRequest.FoundSongs = new List<SongModel.Song>();

            // Act
            var response = playSongRequest.GenerateMessage();

            // Assert
            Assert.Contains("No song found for 328", response);
        }
        
        [Fact]
        public void Play_song_request__generate_message__found_multiple_song() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;
            playSongRequest.FoundSongs = new List<SongModel.Song> {
                SongFixtures.Song2,
                SongFixtures.Song1,
                SongFixtures.Song3
            };

            // Act
            var response = playSongRequest.GenerateMessage();

            // Assert
            Assert.Contains("More than one song found for 328", response);
        }
        
        [Fact]
        public static void Play_song_request__get_song_info_requested__found_all_slots() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            var intentSlots = new Dictionary<string, Slot> {
                { 
                    "SongNumber", new Slot {
                        Name   = "SongNumber",
                        Value = "328"
                    }
                }

            };

            // Act
            playSongRequest.GetSongInfoRequested(intentSlots);

            // Assert
            Assert.Equal("328", playSongRequest.SongRequested.SongNumber);
            Assert.Null(playSongRequest.SongRequested.Artist);
            Assert.Null(playSongRequest.SongRequested.Title);
        }

        [Fact]
        public static void Play_song_request__get_song_info_requested__found_no_slots() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            var intentSlots = new Dictionary<string, Slot> {
                { 
                    "Title", new Slot {
                        Name   = "Title",
                        Value = "I Will Wait"
                    }
                }

            };

            // Act
            playSongRequest.GetSongInfoRequested(intentSlots);

            // Assert
            Assert.Null(playSongRequest.SongRequested.Title);
            Assert.Null(playSongRequest.SongRequested.Artist);
            Assert.Null(playSongRequest.SongRequested.SongNumber);
        }
        
        [Fact]
        public void Play_song_request__find_requested_song__found_one() {

            // Arrange
            IEnumerable<SongModel.Song> foundDynamodbSongs = new List<SongModel.Song> {
                SongFixtures.Song1
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByNumberAsync("328")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;


            // Act
            playSongRequest.FindRequestedSong();

            // Assert
            Assert.Equal("I Will Wait", playSongRequest.FoundSongs.ToList().FirstOrDefault().Title);
            Assert.Equal("Mumford & Sons", playSongRequest.FoundSongs.ToList().FirstOrDefault().Artist);
            Assert.Equal("328", playSongRequest.FoundSongs.ToList().FirstOrDefault().SongNumber);
        }
        
        [Fact]
        public void Play_song_request__find_requested_song__found_multiple() {

            // Arrange
            IEnumerable<SongModel.Song> foundDynamodbSongs = new List<SongModel.Song> {
                SongFixtures.Song1,
                SongFixtures.Song2,
                SongFixtures.Song3
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByNumberAsync("328")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongRequest.SongRequested = SongFixtures.Song1;


            // Act
            playSongRequest.FindRequestedSong();

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
                    Name = "PlaySongNumberRequest",
                    Slots = new Dictionary<string, Slot> {
                        {
                            "SongNumber", new Slot {
                                Name = "SongNumber",
                                Value = "328"
                            }
                        }
                    }
                },
                DialogState = "STARTED",
                Type = "PlaySongTitleArtistRequest"
            };
            
            // mock dependency provider common
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            
            // mock dependency provider sqs
            var request = new JukeboxSqsRequest {
                Key = "328",
                MessageBody = "foo-bar",
                RequestType = "PlaySongNumberRequest"
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
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByNumberAsync("328")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongRequest = new PlaySongNumberRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            
            // Act
            var response = await playSongRequest.HandleRequest(customSkillRequest);
            
            // Assert
            Assert.Contains("Sending song number 328", response.Message); 
        }
    }
}