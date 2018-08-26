using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.SQS;
using Amazon.SQS.Model;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using JukeboxAlexa.Library.TestFixture;
using JukeboxAlexa.Library.Tests;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace JukeboxAlexa.PlaySongTitleArtistRequest.Tests {
    public class PlaySongTitleArtistRequestTest {
        public SongFixtures songFixtures = new SongFixtures();
        
        [Fact]
        public void Play_song_artist_request__is_valid_request__valid() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongArtistRequest.SongRequested = songFixtures.song1;

            // Act
            var response = playSongArtistRequest.IsValidRequest();

            // Assert
            Assert.True(response);
        }
  
        [Fact]
        public void Play_song_artist_request__is_valid_request__invalid() {

            // Arrange
            var tempSongFixtures = new SongFixtures();
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongArtistRequest.SongRequested = tempSongFixtures.song1;
            playSongArtistRequest.SongRequested.Artist = "";

            // Act
            var response = playSongArtistRequest.IsValidRequest();

            // Assert
            Assert.False(response);
        }

        [Fact]
        public void Play_song_artist_request__generate_message__found_one_song() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongArtistRequest.SongRequested = songFixtures.song1;
            playSongArtistRequest.FoundSongs = new List<SongModel.Song> {
                songFixtures.song1
            };

            // Act
            var response = playSongArtistRequest.GenerateMessage();

            // Assert
            Assert.Contains("Sending song number 328", response);
        }

        [Fact]
        public void Play_song_artist_request__generate_message__found_no_song() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongArtistRequest.SongRequested = songFixtures.song1;
            playSongArtistRequest.FoundSongs = new List<SongModel.Song>();

            // Act
            var response = playSongArtistRequest.GenerateMessage();

            // Assert
            Assert.Contains("No song found for I Will Wait", response);
        }
        
        [Fact]
        public void Play_song_artist_request__generate_message__found_multiple_song() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongArtistRequest.SongRequested = songFixtures.song1;
            playSongArtistRequest.FoundSongs = new List<SongModel.Song> {
                songFixtures.song2,
                songFixtures.song1,
                songFixtures.song3
            };

            // Act
            var response = playSongArtistRequest.GenerateMessage();

            // Assert
            Assert.Contains("More than one song found for I Will Wait", response);
        }
        
        [Fact]
        public void Play_song_artist_request__get_song_info_requested__found_all_slots() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            var intentSlots = new Dictionary<string, Slot> {
                { 
                    "Title", new Slot {
                        Name   = "Title",
                        Value = "I Will Wait"
                    }
                },
                { 
                    "ArtistName", new Slot {
                        Name   = "ArtistName",
                        Value = "Mumford & Sons"
                    }
                }

            };

            // Act
            playSongArtistRequest.GetSongInfoRequested(intentSlots);

            // Assert
            Assert.Equal("Mumford & Sons", playSongArtistRequest.SongRequested.Artist);
            Assert.Equal("I Will Wait", playSongArtistRequest.SongRequested.Title);
            Assert.Null(playSongArtistRequest.SongRequested.SongNumber);
        }

        [Fact]
        public void Play_song_artist_request__get_song_info_requested__found_no_slots() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            var intentSlots = new Dictionary<string, Slot> {
                { 
                    "Title", new Slot {
                        Name   = "Title",
                        Value = "I Will Wait"
                    }
                }

            };

            // Act
            playSongArtistRequest.GetSongInfoRequested(intentSlots);

            // Assert
            Assert.Equal("I Will Wait", playSongArtistRequest.SongRequested.Title);
            Assert.Null(playSongArtistRequest.SongRequested.Artist);
            Assert.Null(playSongArtistRequest.SongRequested.SongNumber);
        }
        
        [Fact]
        public void Play_song_artist_request__find_requested_song__found_one() {

            // Arrange
            IEnumerable<SongModel.Song> foundDynamodbSongs = new List<SongModel.Song> {
                songFixtures.song1
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByTitleArtistAsync("I Will Wait", "Mumford & Sons")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongArtistRequest.SongRequested = songFixtures.song1;


            // Act
            playSongArtistRequest.FindRequestedSong();

            // Assert
            Assert.Equal("I Will Wait", playSongArtistRequest.FoundSongs.ToList().FirstOrDefault().Title);
            Assert.Equal("Mumford & Sons", playSongArtistRequest.FoundSongs.ToList().FirstOrDefault().Artist);
            Assert.Equal("328", playSongArtistRequest.FoundSongs.ToList().FirstOrDefault().SongNumber);
        }
        
        [Fact]
        public void Play_song_artist_request__find_requested_song__found_multiple() {

            // Arrange
            IEnumerable<SongModel.Song> foundDynamodbSongs = new List<SongModel.Song> {
                songFixtures.song1,
                songFixtures.song2,
                songFixtures.song3
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByTitleArtistAsync("I Will Wait", "Mumford & Sons")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongArtistRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            playSongArtistRequest.SongRequested = songFixtures.song1;


            // Act
            playSongArtistRequest.FindRequestedSong();

            // Assert
            Assert.Equal("I Will Wait", playSongArtistRequest.FoundSongs.ToList().FirstOrDefault().Title);
            Assert.Equal("Mumford & Sons", playSongArtistRequest.FoundSongs.ToList().FirstOrDefault().Artist);
            Assert.Equal("328", playSongArtistRequest.FoundSongs.ToList().FirstOrDefault().SongNumber);
        }

        [Fact]
        public async Task Play_song_request__handle_request() {
            
            // Arrange
            var customSkillRequest = new CustomSkillRequest {
                Intent = new Intent {
                    Name = "PlaySongTitleArtistRequest",
                    Slots = new Dictionary<string, Slot> {
                        { 
                            "Title", new Slot {
                                Name   = "Title",
                                Value = "I Will Wait"
                            }
                        },
                        { 
                            "ArtistName", new Slot {
                                Name   = "ArtistName",
                                Value = "Mumford & Sons"
                            }
                        }
                    }
                },
                DialogState = "STARTED",
                Type = "PlaySongTitleArtistRequest"
            };
            var skillResponse = ResponseBuilder.Tell("Sending song number 328");
            
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
                songFixtures.song1
            };
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbFindSongsByTitleArtistAsync("I Will Wait", "Mumford & Sons")).Returns(Task.FromResult(foundDynamodbSongs));
            var playSongRequest = new PlaySongTitleArtistRequest(provider.Object, sqsClient.Object, "http://foo-bar", dynamodbProvider.Object);
            
            // Act
            var response = await playSongRequest.HandleRequest(customSkillRequest);
            
            // Assert
            Assert.Contains("Sending song number 328", response.Message);
        }
    }
}