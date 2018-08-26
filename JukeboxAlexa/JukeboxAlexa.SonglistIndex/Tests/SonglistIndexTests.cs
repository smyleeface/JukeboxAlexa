//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Amazon.DynamoDBv2.Model;
//using Amazon.S3;
//using Amazon.SQS;
//using JukeboxAlexa.Library;
//using JukeboxAlexa.Library.Model;
//using Moq;
//using Newtonsoft.Json;
//using Xunit;
//
//namespace JukeboxAlexa.SonglistIndex.Tests {
//    public class SonglistIndexTests {
//        
//        [Fact]
//        public async Task Songlist_index__insert_songs__none_found() {
//            
//            // Arrange
//            var song = new SongCsvModel {
//                SearchArtist = "foo artist",
//                SearchTitle = "foo title",
//                SongNumber = "123"
//            };
//            var song2 = new SongCsvModel {
//                SearchArtist = "foo artist2",
//                SearchTitle = "foo title2",
//                SongNumber = "223"
//            };
//            var song3 = new SongCsvModel {
//                SearchArtist = "foo artist3",
//                SearchTitle = "foo title3",
//                SongNumber = "323"
//            };
//            var listOfSongs = new List<SongCsvModel> {
//                song
//            };
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            dynamodbProvider.Setup(x => x.DynamodbGetItemAsync(It.Is<Dictionary<string, AttributeValue>>(y => 
//                y.GetValueOrDefault("word").S == "Foo"
//            ))).Returns(Task.FromResult(new GetItemResponse {
//                Item = new Dictionary<string, AttributeValue> {
//                    {
//                        "word", new AttributeValue {
//                            S = "Foo"
//                        }
//                    },
//                    {
//                        "song", new AttributeValue {
//                            S = JsonConvert.SerializeObject(listOfSongs)
//                        }
//                    }
//                }
//            }));
//            
//            dynamodbProvider.Setup(x => x.DynamodbGetItemAsync(It.Is<Dictionary<string, AttributeValue>>(y => 
//                y.GetValueOrDefault("word").S == "Title"
//            ))).Returns(Task.FromResult(new GetItemResponse {
//                Item = new Dictionary<string, AttributeValue> {
//                    {
//                        "word", new AttributeValue {
//                            S = "Title"
//                        }
//                    },
//                    {
//                        "song", new AttributeValue {
//                            S = JsonConvert.SerializeObject(listOfSongs)
//                        }
//                    }
//                }
//            }));
//
//            //            dynamodbProvider.Setup(x => x.DynamodbUpdateItemAsync(key, updateExpression, expressionAttributeValues)).Returns(Task.FromResult(new UpdateItemResponse()));
//            
//            dynamodbProvider.Setup(x => x.DynamodbPutItemAsync(It.Is<Dictionary<string, AttributeValue>>(y => 
//                y.GetValueOrDefault(":n").S == JsonConvert.SerializeObject(song)
//            ))).Returns(Task.FromResult(new PutItemResponse()));
//            
//            var songlistUpload = new SonglistIndex(dynamodbProvider.Object) {
//                bucketName = "foo",
//                keyName = "bar"
//            };
//
//            // Act
//            // Assert
//            await songlistUpload.InsertSongs("Foo Title", "Foo Artist", "123");
//            
//            
//        }
//
//        [Fact]
//        public async Task Songlist_index__insert_songs() {
//            
//            // Arrange
//            var song = new SongCsvModel {
//                SearchArtist = "foo artist",
//                SearchTitle = "foo title",
//                SongNumber = "123"
//            };
//            var song2 = new SongCsvModel {
//                SearchArtist = "foo artist2",
//                SearchTitle = "foo title2",
//                SongNumber = "223"
//            };
//            var song3 = new SongCsvModel {
//                SearchArtist = "foo artist3",
//                SearchTitle = "foo title3",
//                SongNumber = "323"
//            };
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            dynamodbProvider.Setup(x => x.DynamodbGetItemAsync(It.Is<Dictionary<string, AttributeValue>>(y => 
//                y.GetValueOrDefault("word").S == "Foo"
//            ))).Returns(Task.FromResult(new GetItemResponse()));
//            
//            dynamodbProvider.Setup(x => x.DynamodbGetItemAsync(It.Is<Dictionary<string, AttributeValue>>(y => 
//                y.GetValueOrDefault("word").S == "Title"
//            ))).Returns(Task.FromResult(new GetItemResponse()));
//
//            //            dynamodbProvider.Setup(x => x.DynamodbUpdateItemAsync(key, updateExpression, expressionAttributeValues)).Returns(Task.FromResult(new UpdateItemResponse()));
//            
//            dynamodbProvider.Setup(x => x.DynamodbPutItemAsync(It.Is<Dictionary<string, AttributeValue>>(y => 
//                y.GetValueOrDefault(":n").S == JsonConvert.SerializeObject(song)
//            ))).Returns(Task.FromResult(new PutItemResponse()));
//            
//            var songlistUpload = new SonglistIndex(dynamodbProvider.Object) {
//                bucketName = "foo",
//                keyName = "bar"
//            };
//
//            // Act
//            // Assert
//            await songlistUpload.InsertSongs("Foo Title", "Foo Artist", "123");
//            
//            
//        }
//    }
//}