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
    public class DynamoDbFindSongsByNumberTest {
        public SongFixtures SongFixtures = new SongFixtures();
        
        [Fact]
        public async Task Find_songs_by_number__found_one_song() {
            
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
            var parsedSongsList = (await jukeboxDynamoDb.FindSongsByNumberAsync("123")).ToList();

            // Assert
            Assert.Equal("Lionel Ritche", parsedSongsList.FirstOrDefault().Artist);
            Assert.Equal("123", parsedSongsList.FirstOrDefault().SongNumber);
            Assert.Equal("Hello", parsedSongsList.FirstOrDefault().Title);
        }

        [Fact]
        public async Task Find_songs_by_title_artist__not_found_artist() {
            
            // Arrange
            var queryResponse = new QueryResponse {
                Items = new List<Dictionary<string, AttributeValue>>()
            };
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            dynamodbClient.Setup(x => x.QueryAsync(It.Is<QueryRequest>(y => y.TableName == SongFixtures.TableName), new CancellationToken())).Returns(Task.FromResult(queryResponse));
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var parsedSongsList = await jukeboxDynamoDb.FindSongsByNumberAsync("555");

            // Assert
            Assert.Empty(parsedSongsList);
        }

        [Fact]
        public void Query_request__number() {
           
            // Setup
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var query = jukeboxDynamoDb.QueryRequestNumber("123");

            // Assert
            Assert.Contains("track_number = :v_number", query.KeyConditionExpression);
            query.ExpressionAttributeValues.TryGetValue(":v_number", out AttributeValue vNumber);
            Assert.Contains("123", vNumber.S);
        }
        
    }
}