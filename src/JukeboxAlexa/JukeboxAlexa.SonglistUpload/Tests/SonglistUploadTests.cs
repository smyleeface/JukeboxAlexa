using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.Model;
using Moq;
using Xunit;

namespace JukeboxAlexa.SonglistUpload.Tests {
    public class SonglistUploadTests {
        
        [Fact]
        public async Task Songlist_upload__read_new_songs__found() {
            
            // Arrange
            var returnedSongs = "3,19,Neon Trees,Animals,Neon Trees,3,19,,,,,,\n2,13,Trees,Animals,Trees,2,13,,,,,,\n1,04,Adelle,Hello,Adelle,1,04,,,,,,";
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            s3Provider.Setup(x => x.GetSongsFromS3UploadAsync("foo", "bar")).Returns(Task.FromResult(returnedSongs));
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object);
            songlistUpload.BucketName = "foo";
            songlistUpload.KeyName = "bar";
            
            // Act
            await songlistUpload.ReadNewSongs();
            
            // Assert
            Assert.Contains(songlistUpload.NewSongs, x => x.Artist == "Neon Trees");
            Assert.Contains(songlistUpload.NewSongs, x => x.SearchArtist == "neon trees");
            Assert.Contains(songlistUpload.NewSongs, x => x.SongNumber == "319");
        }

        [Fact]
        public async Task Songlist_upload__read_new_songs__found_with_empty_and_non_digit() {
            
            // Arrange
            var returnedSongs = "3,19,Neon Trees,Animals,Neon Trees,3,19,,,,,,\n,,,,,,,,,,,,\n2,13,Trees,Animals,Trees,2,13,,,,,,\ntwo,three,Trees,Animals,Trees,two,three,,,,,,\n1,04,Adelle,Hello,Adelle,1,04,,,,,,";
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            s3Provider.Setup(x => x.GetSongsFromS3UploadAsync("foo", "bar")).Returns(Task.FromResult(returnedSongs));
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar"
            };

            // Act
            await songlistUpload.ReadNewSongs();
            
            // Assert
            Assert.Equal(3, songlistUpload.NewSongs.Count());
        }

        [Fact]
        public async Task Songlist_upload__read_old_songs__found() {
            
            // Arrange
            var scanResults = new ScanResponse {
                Items = new List<Dictionary<string, AttributeValue>> {
                    new Dictionary<string, AttributeValue> {
                        { "artist", new AttributeValue { S = "Foo-Artist" }},
                        { "search_artist", new AttributeValue { S = "foo-artist" }},
                        { "song_number", new AttributeValue { S = "123" }},
                        { "title", new AttributeValue { S = "Foo-Title" }},
                        { "search_title", new AttributeValue { S = "foo-title" }}
                    }
                }
            };
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbScanAsync()).Returns(Task.FromResult(scanResults));
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar"
            };

            // Act
            await songlistUpload.ReadOldSongs();
            
            // Assert
            Assert.Contains(songlistUpload.OldSongs, x => x.Artist == "Foo-Artist");
            Assert.Contains(songlistUpload.OldSongs, x => x.SearchArtist == "foo-artist");
            Assert.Contains(songlistUpload.OldSongs, x => x.SongNumber == "123");
        }

        [Fact]
        public async Task Songlist_upload__read_old_songs__none_found() {
            
            // Arrange
            var scanResults = new ScanResponse {
                Items = new List<Dictionary<string, AttributeValue>> {
                    new Dictionary<string, AttributeValue>()
                }
            };
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbScanAsync()).Returns(Task.FromResult(scanResults));
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar"
            };

            // Act
            await songlistUpload.ReadOldSongs();
            
            // Assert
            Assert.Empty(songlistUpload.OldSongs);
        }
        
        [Fact]
        public void Songlist_upload__song_to_add__found_one() {
            
            // Arrange
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar",
                NewSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist",
                        SearchArtist = "foo-artist",
                        SongNumber = "123",
                        Title = "Foo-Title",
                        SearchTitle = "foo-title"
                    }
                },
                OldSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist2",
                        SearchArtist = "foo-artist2",
                        SongNumber = "124",
                        Title = "Foo-Title2",
                        SearchTitle = "foo-title2"
                    }
                },
            };

            // Act
            songlistUpload.FilterSongsToAdd();
            
            // Assert
            Assert.Contains(songlistUpload.SongsToAdd, x => x.Artist == "Foo-Artist");
            Assert.Contains(songlistUpload.SongsToAdd, x => x.SearchArtist == "foo-artist");
            Assert.Contains(songlistUpload.SongsToAdd, x => x.SongNumber == "123");
        }


        [Fact]
        public void Songlist_upload__song_to_add__found_none() {

            // Arrange
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar",
                NewSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist2",
                        SearchArtist = "foo-artist2",
                        SongNumber = "123",
                        Title = "Foo-Title2",
                        SearchTitle = "foo-title2"
                    }
                },
                OldSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist2",
                        SearchArtist = "foo-artist2",
                        SongNumber = "123",
                        Title = "Foo-Title2",
                        SearchTitle = "foo-title2"
                    }
                },
            };

            // Act
            songlistUpload.FilterSongsToAdd();

            // Assert
            Assert.Empty(songlistUpload.SongsToAdd);
        }
        
        [Fact]
        public void Songlist_upload__song_to_delete__found_one() {
            
            // Arrange
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar",
                NewSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist",
                        SearchArtist = "foo-artist",
                        SongNumber = "123",
                        Title = "Foo-Title",
                        SearchTitle = "foo-title"
                    }
                },
                OldSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist2",
                        SearchArtist = "foo-artist2",
                        SongNumber = "124",
                        Title = "Foo-Title2",
                        SearchTitle = "foo-title2"
                    }
                },
            };

            // Act
            songlistUpload.FilterSongsToDelete();
            
            // Assert
            Assert.Contains(songlistUpload.SongsToDelete, x => x.Artist == "Foo-Artist2");
            Assert.Contains(songlistUpload.SongsToDelete, x => x.SearchArtist == "foo-artist2");
            Assert.Contains(songlistUpload.SongsToDelete, x => x.SongNumber == "124");
        }

        [Fact]
        public void Songlist_upload__song_to_delete__found_none() {
            
            // Arrange
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar",
                NewSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist2",
                        SearchArtist = "foo-artist2",
                        SongNumber = "124",
                        Title = "Foo-Title2",
                        SearchTitle = "foo-title2"
                    }
                },
                OldSongs = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist2",
                        SearchArtist = "foo-artist2",
                        SongNumber = "124",
                        Title = "Foo-Title2",
                        SearchTitle = "foo-title2"
                    }
                },
            };

            // Act
            songlistUpload.FilterSongsToDelete();
            
            // Assert
            Assert.Empty(songlistUpload.SongsToDelete);
        }

        [Fact]
        public async Task Songlist_upload__delete_song_from_database() {

            // Arrange
            var dynamodbValues = new List<WriteRequest>();
            var dbRecord = new WriteRequest {
                DeleteRequest = new DeleteRequest {
                    Key = new Dictionary<string, AttributeValue> {
                        { "song_number", new AttributeValue {
                            S = "124"
                        }}
                    }
                }
            };
            dynamodbValues.Add(dbRecord);
            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
            dynamodbProvider.Setup(x => x.DynamoDbBatchWriteItemAsync(It.Is<List<WriteRequest>>(y => 
                y.FirstOrDefault().DeleteRequest.Key.ToList()[0].Value.S == "Foo-Title2" && 
                y.FirstOrDefault().DeleteRequest.Key.ToList()[1].Value.S == "Foo-Artist2"
                ))).Returns(Task.FromResult(new BatchWriteItemResponse()));
            dynamodbProvider.Setup(x => x.DynamoDbBatchWriteItemAsync(It.Is<List<WriteRequest>>(y => 
                y.FirstOrDefault().DeleteRequest.Key.GetValueOrDefault("song_number").S == "124"
                ))).Returns(Task.FromResult(new BatchWriteItemResponse()));
            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
            var songlistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object) {
                BucketName = "foo",
                KeyName = "bar",
                SongsToDelete = new List<SongCsvModel> {
                    new SongCsvModel {
                        Artist = "Foo-Artist2",
                        SearchArtist = "foo-artist2",
                        SongNumber = "124",
                        Title = "Foo-Title2",
                        SearchTitle = "foo-title2"
                    }
                }
            };

            // Act
            // Assert
            await songlistUpload.DeleteSongsFromDatabase();
        }

    }
}