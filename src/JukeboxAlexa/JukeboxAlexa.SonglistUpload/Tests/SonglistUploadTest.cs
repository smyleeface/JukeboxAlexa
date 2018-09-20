//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Amazon.DynamoDBv2.Model;
//using Amazon.S3.Model;
//using JukeboxAlexa.Library.Model;
//using Moq;
//using Xunit;
//
//namespace JukeboxAlexa.SonglistUpload.Tests {
//    public class SonglistUploadTest {
//        
//        [Fact]
//        public static async Task Songlist_upload__read_new_songs__found() {
//            
//            // Arrange
//            var memoryStream = new MemoryStream();
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            s3Provider.Setup(x => x.S3GetObjectAsync("foo", "bar")).Returns(Task.FromResult(new GetObjectResponse {
//                BucketName = "foo",
//                Key = "bar",
//                ResponseStream = memoryStream
//            }));
//            s3Provider.Setup(x => x.ReadS3Stream(memoryStream)).Returns("Disc,Track#,Song,Artist,Disc,Track#,,,,,,\n3,05,Rihanna,Stay,Rihanna,3,05,,,,,,");
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object);
//            songlistUpload.BucketName = "foo";
//            songlistUpload.KeyName = "bar";
//            
//            // Act
//            await songlistUpload.ReadNewSongs();
//            
//            // Assert
//            Assert.Contains(songlistUpload.NewSongs, x => x.Artist == "Rihanna");
//            Assert.Contains(songlistUpload.NewSongs, x => x.SearchArtist == "rihanna");
//            Assert.Contains(songlistUpload.NewSongs, x => x.SongNumber == "305");
//        }
//
//        [Fact]
//        public static async Task Songlist_upload__read_new_songs__found_with_empty_and_non_digit() {
//            
//            // Arrange
//            var memoryStream = new MemoryStream();
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            s3Provider.Setup(x => x.S3GetObjectAsync("foo", "bar")).Returns(Task.FromResult(new GetObjectResponse {
//                BucketName = "foo",
//                Key = "bar",
//                ResponseStream = memoryStream
//            }));
//            s3Provider.Setup(x => x.ReadS3Stream(memoryStream)).Returns("Disc,Track#,Song,Artist,Disc,Track#,,,,,,\n3,05,Rihanna,Stay,Rihanna,3,05,,,,,,\n1,05,Neon Tress,Hello,Neon Trees,1,05,,,,,,");
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar"
//            };
//
//            // Act
//            await songlistUpload.ReadNewSongs();
//            
//            // Assert
//            Assert.Equal(2, songlistUpload.NewSongs.Count());
//        }
//
//        [Fact]
//        public static async Task Songlist_upload__read_old_songs__found() {
//            
//            // Arrange
//            var scanResults = new ScanResponse {
//                Items = new List<Dictionary<string, AttributeValue>> {
//                    new Dictionary<string, AttributeValue> {
//                        { "artist", new AttributeValue { S = "Foo-Artist" }},
//                        { "search_artist", new AttributeValue { S = "foo-artist" }},
//                        { "song_number", new AttributeValue { S = "123" }},
//                        { "title", new AttributeValue { S = "Foo-Title" }},
//                        { "search_title", new AttributeValue { S = "foo-title" }}
//                    }
//                }
//            };
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            dynamodbProvider.Setup(x => x.DynamoDbScanAsync()).Returns(Task.FromResult(scanResults));
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar"
//            };
//
//            // Act
//            await songlistUpload.ReadOldSongs();
//            
//            // Assert
//            Assert.Contains(songlistUpload.OldSongs, x => x.Artist == "Foo-Artist");
//            Assert.Contains(songlistUpload.OldSongs, x => x.SearchArtist == "foo-artist");
//            Assert.Contains(songlistUpload.OldSongs, x => x.SongNumber == "123");
//        }
//
//        [Fact]
//        public static async Task Songlist_upload__read_old_songs__none_found() {
//            
//            // Arrange
//            var scanResults = new ScanResponse {
//                Items = new List<Dictionary<string, AttributeValue>> {
//                    new Dictionary<string, AttributeValue>()
//                }
//            };
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            dynamodbProvider.Setup(x => x.DynamoDbScanAsync()).Returns(Task.FromResult(scanResults));
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar"
//            };
//
//            // Act
//            await songlistUpload.ReadOldSongs();
//            
//            // Assert
//            Assert.Empty(songlistUpload.OldSongs);
//        }
//        
//        [Fact]
//        public static void Songlist_upload__song_to_add__found_one() {
//            
//            // Arrange
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar",
//                NewSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist",
//                        SearchArtist = "foo-artist",
//                        SongNumber = "123",
//                        Title = "Foo-Title",
//                        SearchTitle = "foo-title"
//                    }
//                },
//                OldSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "124",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                },
//            };
//
//            // Act
//            songlistUpload.FilterSongsToAdd();
//            
//            // Assert
//            Assert.Contains(songlistUpload.SongsToAdd, x => x.Artist == "Foo-Artist");
//            Assert.Contains(songlistUpload.SongsToAdd, x => x.SearchArtist == "foo-artist");
//            Assert.Contains(songlistUpload.SongsToAdd, x => x.SongNumber == "123");
//        }
//
//
//        [Fact]
//        public static void Songlist_upload__song_to_add__found_none() {
//
//            // Arrange
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar",
//                NewSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "123",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                },
//                OldSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "123",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                },
//            };
//
//            // Act
//            songlistUpload.FilterSongsToAdd();
//
//            // Assert
//            Assert.Empty(songlistUpload.SongsToAdd);
//        }
//        
//        [Fact]
//        public static void Songlist_upload__song_to_delete__found_one() {
//            
//            // Arrange
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar",
//                NewSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist",
//                        SearchArtist = "foo-artist",
//                        SongNumber = "123",
//                        Title = "Foo-Title",
//                        SearchTitle = "foo-title"
//                    }
//                },
//                OldSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "124",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                },
//            };
//
//            // Act
//            songlistUpload.FilterSongsToDelete();
//            
//            // Assert
//            Assert.Contains(songlistUpload.SongsToDelete, x => x.Artist == "Foo-Artist2");
//            Assert.Contains(songlistUpload.SongsToDelete, x => x.SearchArtist == "foo-artist2");
//            Assert.Contains(songlistUpload.SongsToDelete, x => x.SongNumber == "124");
//        }
//
//        [Fact]
//        public static void Songlist_upload__song_to_delete__found_none() {
//            
//            // Arrange
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar",
//                NewSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "124",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                },
//                OldSongs = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "124",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                },
//            };
//
//            // Act
//            songlistUpload.FilterSongsToDelete();
//            
//            // Assert
//            Assert.Empty(songlistUpload.SongsToDelete);
//        }
//
//        [Fact]
//        public static async Task Songlist_upload__delete_song_from_database() {
//
//            // Arrange
//            var dynamodbValues = new List<WriteRequest>();
//            var dbRecord = new WriteRequest {
//                DeleteRequest = new DeleteRequest {
//                    Key = new Dictionary<string, AttributeValue> {
//                        { "song_number", new AttributeValue {
//                            S = "124"
//                        }}
//                    }
//                }
//            };
//            dynamodbValues.Add(dbRecord);
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            dynamodbProvider.Setup(x => x.DynamoDbBatchWriteItemAsync(It.Is<List<WriteRequest>>(y => 
//                y.FirstOrDefault().DeleteRequest.Key.ToList()[0].Value.S == "Foo-Title2" && 
//                y.FirstOrDefault().DeleteRequest.Key.ToList()[1].Value.S == "Foo-Artist2"
//                ))).Returns(Task.FromResult(new BatchWriteItemResponse()));
//            dynamodbProvider.Setup(x => x.DynamoDbBatchWriteItemAsync(It.Is<List<WriteRequest>>(y => 
//                y.FirstOrDefault().DeleteRequest.Key.GetValueOrDefault("song_number").S == "124"
//                ))).Returns(Task.FromResult(new BatchWriteItemResponse()));
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar",
//                SongsToDelete = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "124",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                }
//            };
//
//            // Act
//            // Assert
//            await songlistUpload.DeleteSongsFromDatabase();
//        }
//
//        [Fact]
//        public static async Task Songlist_upload__add_songs_to_dataabase() {
//            
//            // Arrange
//            var dynamodbValues = new List<WriteRequest>();
//            var dbRecord = new WriteRequest {
//                PutRequest = new PutRequest {
//                    Item = new Dictionary<string, AttributeValue> {
//                        { "song_number", new AttributeValue {
//                            S = "124"
//                        }},
//                        { "search_artist", new AttributeValue {
//                            S = "foo-artist2"
//                        }},
//                        { "artist", new AttributeValue {
//                            S = "Foo-Artist2"
//                        }},
//                        { "search_title", new AttributeValue {
//                            S = "foo-title2"
//                        }},
//                        { "title", new AttributeValue {
//                            S = "Foo-Title2"
//                        }}
//                    }
//                }
//            };
//            dynamodbValues.Add(dbRecord);
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            dynamodbProvider.Setup(x => x.DynamoDbBatchWriteItemAsync(It.Is<List<WriteRequest>>(y => 
//                y.FirstOrDefault().PutRequest.Item.ToList()[0].Value.S == "Foo-Title2" && 
//                y.FirstOrDefault().PutRequest.Item.ToList()[1].Value.S == "Foo-Artist2"
//                ))).Returns(Task.FromResult(new BatchWriteItemResponse()));
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
//                BucketName = "foo",
//                KeyName = "bar",
//                SongsToAdd = new List<SongCsvModel> {
//                    new SongCsvModel {
//                        Artist = "Foo-Artist2",
//                        SearchArtist = "foo-artist2",
//                        SongNumber = "124",
//                        Title = "Foo-Title2",
//                        SearchTitle = "foo-title2"
//                    }
//                }
//            };
//            
//            // Act
//            // Assert
//            await songlistUpload.AddSongsToDatabase();
//        }
//    }
//}