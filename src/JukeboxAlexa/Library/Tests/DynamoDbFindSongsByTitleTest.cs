﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.TestFixtures;
using Moq;
using Xunit;

namespace JukeboxAlexa.Library.Tests {
    public class DynamoDbFindSongsByTitleTest {
        public SongFixtures SongFixtures = new SongFixtures();

        [Fact]
        public async Task Find_songs_by_title__found_one_song() {
            
            // Arrange
            var queryResponse = new QueryResponse {
                Items = new List<Dictionary<string, AttributeValue>> {
                    new Dictionary<string, AttributeValue> {
                        {"song_number", new AttributeValue {S = "123"}},
                        {"artist", new AttributeValue {S = "Lionel Ritche"}},
                        {"title", new AttributeValue {S = "Hello"}}
                    }
                }
            };
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            dynamodbClient.Setup(x => x.QueryAsync(It.Is<QueryRequest>(y => y.TableName == SongFixtures.TableName), new CancellationToken())).Returns(Task.FromResult(queryResponse));
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var parsedSongsList = (await jukeboxDynamoDb.FindSongsByTitleAsync("hello")).ToList();

            // Assert
            Assert.Equal("Lionel Ritche", parsedSongsList.FirstOrDefault().Artist);
            Assert.Equal("123", parsedSongsList.FirstOrDefault().SongNumber);
            Assert.Equal("Hello", parsedSongsList.FirstOrDefault().Title);
        }
        
        [Fact]
        public async Task Find_songs_by_title__found_multiple_songs() {
            
            // Arrange
            var queryResponse = new QueryResponse {
                Items = new List<Dictionary<string, AttributeValue>> {
                    new Dictionary<string, AttributeValue> {
                        {"song_number", new AttributeValue {S = "123"}},
                        {"artist", new AttributeValue {S = "Lionel Ritche"}},
                        {"title", new AttributeValue {S = "Hello"}}
                    },
                    new Dictionary<string, AttributeValue> {
                        {"song_number", new AttributeValue {S = "789"}},
                        {"artist", new AttributeValue {S = "Adele"}},
                        {"title", new AttributeValue{S = "Hello"}}
                    }
                }
            };
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            dynamodbClient.Setup(x => x.QueryAsync(It.Is<QueryRequest>(y => y.TableName == SongFixtures.TableName), new CancellationToken())).Returns(Task.FromResult(queryResponse));
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var parsedSongsList = (await jukeboxDynamoDb.FindSongsByTitleAsync("hello")).ToList();
    
            // Assert
            foreach (var song in parsedSongsList) {
                if (song.Artist == "Lionel Ritche") {
                    Assert.True(true);
                }
            }
            
            var foundArtist1 = parsedSongsList.Find(x => x.Artist == "Lionel Ritche").Artist;
            var foundArtist2 = parsedSongsList.Find(x => x.Artist == "Adele").Artist;
            Assert.Equal("Lionel Ritche", foundArtist1);
            Assert.Equal("Adele", foundArtist2);
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
            var parsedSongsList = await jukeboxDynamoDb.FindSongsByTitleAsync("helloooooo");

            // Assert
            Assert.Empty(parsedSongsList);
        }
        
        [Fact]
        public void Query_request__title() {
           
            // Arrange
            var dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            var jukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, SongFixtures.TableName, SongFixtures.IndexNameSearchTitle, SongFixtures.IndexNameSearchTitleArtist, SongFixtures.TableName);
            
            // Act
            var query = jukeboxDynamoDb.QueryRequestTitle("foo-bar");

            // Assert
            Assert.Contains("search_title = :v_song", query.KeyConditionExpression);
            query.ExpressionAttributeValues.TryGetValue(":v_song", out AttributeValue vSong);
            Assert.Contains("foo-bar", vSong.S);
        }
    }
}