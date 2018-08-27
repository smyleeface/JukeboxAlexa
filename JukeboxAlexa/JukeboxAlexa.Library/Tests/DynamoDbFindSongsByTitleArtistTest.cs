using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.TestFixtures;
using Moq;
using Xunit;

namespace JukeboxAlexa.Library.Tests {
    public class DynamoDbFindSongsByTitleArtistTest {
        public SongFixtures SongFixtures = new SongFixtures();
        
        [Fact]
        public async void Find_songs_by_title_artist__found_song() {
            
            // Arrange
            var queryResponse = new QueryResponse {
                Items = new List<Dictionary<string, AttributeValue>> {
                    new Dictionary<string, AttributeValue> {
                        {"track_number", new AttributeValue {S = "123"}},
                        {"artist", new AttributeValue {S = "Lionel Ritche"}},
                        {"title", new AttributeValue {S = "Hello"}}
                    }
                }
            };
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            dynamodbClient.Setup(x => x.QueryAsync(It.Is<QueryRequest>(y => y.TableName == SongFixtures.TableName), new CancellationToken())).Returns(Task.FromResult(queryResponse));
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var parsedSongsList = (await jukeboxDynamoDb.FindSongsByTitleArtistAsync("i will wait", "mumford & sons")).ToList();

            // Assert
            Assert.Equal("Lionel Ritche", parsedSongsList.FirstOrDefault().Artist);
            Assert.Equal("123", parsedSongsList.FirstOrDefault().SongNumber);
            Assert.Equal("Hello", parsedSongsList.FirstOrDefault().Title);
        }
        
        [Fact]
        public async void Find_songs_by_title_artist__found_no_songs() {
            
            // Arrange
            var queryResponse = new QueryResponse {
                Items = new List<Dictionary<string, AttributeValue>>()
            };
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            dynamodbClient.Setup(x => x.QueryAsync(It.Is<QueryRequest>(y => y.TableName == SongFixtures.TableName), new CancellationToken())).Returns(Task.FromResult(queryResponse));
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var parsedSongsList = await jukeboxDynamoDb.FindSongsByTitleArtistAsync("foo-bar", "foo-bar");

            // Assert
            Assert.Empty(parsedSongsList);
        }
        
        [Fact]
        public void Query_request__title_artist() {
           
            // Setup
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var query = jukeboxDynamoDb.QueryRequestTitleArtist("foo-bar", "bar-baz");

            // Assert
            Assert.Contains("search_title = :v_song AND search_artist = :v_artist", query.KeyConditionExpression);
            query.ExpressionAttributeValues.TryGetValue(":v_song", out AttributeValue vSong);
            Assert.Contains("foo-bar", vSong.S);
            query.ExpressionAttributeValues.TryGetValue(":v_artist", out AttributeValue vArtist);
            Assert.Contains("bar-baz", vArtist.S);
        }
    }
}