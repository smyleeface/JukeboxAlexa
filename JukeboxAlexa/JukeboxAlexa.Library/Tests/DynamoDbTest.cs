using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.TestFixture;
using Moq;
using Xunit;

namespace JukeboxAlexa.Library.Tests {
    public class DynamoDbTest {        
        
        public SongFixtures songFixtures = new SongFixtures();
        
        [Fact]
        public void Parse_songs_from_database_response__one_song() {
            
            // Arrange
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, songFixtures.tableName, songFixtures.indexNameSearchTitle, songFixtures.indexNameSearchTitleArtist);
            var items = new List<Dictionary<string, AttributeValue>> {
                {
                    new Dictionary<string, AttributeValue> {
                        {"track_number", new AttributeValue {S = "123"}},
                        {"artist", new AttributeValue {S = "Lionel Ritche"}},
                        {"title", new AttributeValue {S = "Hello"}}
                    }
                }
            };
            var quesryResponse = new QueryResponse {
                Items = items
            };
            
            // Act
            var parsedSongsList = jukeboxDynamoDb.ParseSongsFromDatabaseResponse(quesryResponse).ToList();

            // Assert
            Assert.Equal("Lionel Ritche", parsedSongsList.FirstOrDefault().Artist);
            Assert.Equal("123", parsedSongsList.FirstOrDefault().Number);
            Assert.Equal("Hello", parsedSongsList.FirstOrDefault().Title);
        }

        [Fact]
        public void Parse_songs_from_database_response__no_songs() {
            
            // Arrange
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, songFixtures.tableName, songFixtures.indexNameSearchTitle, songFixtures.indexNameSearchTitleArtist);
            var items = new List<Dictionary<string, AttributeValue>> {};
            var quesryResponse = new QueryResponse {
                Items = items
            };
            
            // Act
            var parsedSongsList = jukeboxDynamoDb.ParseSongsFromDatabaseResponse(quesryResponse).ToList();

            // Assert
            Assert.Empty(parsedSongsList);
        }

        [Fact]
        public void Parse_songs_from_database_response__multiple_songs() {
            
            // Arrange
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, songFixtures.tableName, songFixtures.indexNameSearchTitle, songFixtures.indexNameSearchTitleArtist);
            var items = new List<Dictionary<string, AttributeValue>> {
                songFixtures.songAttribute2,
                songFixtures.songAttribute3
            };
            var queryResponse = new QueryResponse {
                Items = items
            };
            
            // Act
            var parsedSongsList = jukeboxDynamoDb.ParseSongsFromDatabaseResponse(queryResponse).ToList();

            // Assert
            Assert.Contains(parsedSongsList, e => songFixtures.song2.Artist == e.Artist && songFixtures.song3.Artist == e.Artist);
        }
    }
}