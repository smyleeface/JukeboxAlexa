//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Amazon.DynamoDBv2.Model;
//using Amazon.Lambda.DynamoDBEvents;
//using Moq;
//using Xunit;
//
//namespace JukeboxAlexa.SonglistIndex.Tests {
//    public class SonglistIndexTests {
//        
//        [Fact]
//        public static async Task Songlist_index__insert_songs__none_found() {
//            
//            // Arrange
//            var record = new DynamoDBEvent.DynamodbStreamRecord {
//                Dynamodb = new StreamRecord {
//                    Keys = new Dictionary<string, AttributeValue> {
//                        {
//                            "word", new AttributeValue {
//                                S = "stay"
//                            }
//                        }
//                    },
//                    NewImage = new Dictionary<string, AttributeValue> {
//                        {
//                            "search_title", new AttributeValue {
//                                S = "stay"
//                            }
//                        },
//                        {
//                            "artist", new AttributeValue {
//                                S = "ABC123"
//                            }
//                        },
//                        {
//                            "search_artist", new AttributeValue {
//                                S = "abc123"
//                            }
//                        },
//                        {
//                            "title", new AttributeValue {
//                                S = "Stay"
//                            }
//                        },
//                        {
//                            "song_number", new AttributeValue {
//                                S = "123"
//                            }
//                        }
//                    }
//                },
//                EventName = "INSERT"
//            };
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            
//            dynamodbProvider.Setup(x => x.DynamodbGetItemAsync(It.Is<Dictionary<string, AttributeValue>>(y =>
//                y.GetValueOrDefault("word").S == "stay"
//            ))).Returns(Task.FromResult(new GetItemResponse()));
//            
//            dynamodbProvider.Setup(x => x.DynamodbUpdateItemAsync(
//                It.IsAny<Dictionary<string, AttributeValue>>(),
//                "SET songs = :n",
//                It.IsAny<Dictionary<string, AttributeValue>>()
//            )).Returns(Task.FromResult(new UpdateItemResponse()));
//            
//            var songListIndex = new SonglistIndex(dynamodbProvider.Object);
//            
//            // Act
//            // Assert
//            await songListIndex.HandleRequest(record);
//        }
//        
//    }
//}