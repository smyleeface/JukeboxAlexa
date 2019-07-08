using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using JukeboxAlexa.Library;
using Moq;
using Xunit;

namespace JukeboxAlexa.SonglistUpload.Tests {
    public class SonglistUploadFunctionTest {
//        
//        [Fact]
//        public static async Task Songlist_upload__process_message() {
//
//            // Arrange
//            Mock<IDynamodbDependencyProvider> dynamodbProvider = new Mock<IDynamodbDependencyProvider>(MockBehavior.Strict);
//            Mock<IAmazonDynamoDB> dynamodbClient = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
//            Mock<IS3DependencyProvider> s3Provider = new Mock<IS3DependencyProvider>(MockBehavior.Strict);
//            MemoryStream responseDataStream = new MemoryStream();
//            s3Provider.Setup(x => x.S3GetObjectAsync("foo", "bar")).Returns(Task.FromResult(new GetObjectResponse {
//                ResponseStream = responseDataStream
//            }));
//            s3Provider.Setup(x => x.ReadS3Stream(responseDataStream)).Returns("Disc,Track#,Song,Artist,Disc,Track#,,,,,,\n3,05,Rihanna,Stay,Rihanna,3,05,,,,,,");
//            Mock<IAmazonS3> s3Client = new Mock<IAmazonS3>(MockBehavior.Strict);
//            var function = new Function {
//                JukeboxDynamoDb = new JukeboxDynamoDb(dynamodbClient.Object, "foo", "foo", "foo", "foo"),
//                JukeboxS3 = new JukeboxS3(s3Client.Object),
//                SonglistUpload = new SonglistUpload(dynamodbProvider.Object, s3Provider.Object)
//            };
//            
//            // Act
//            // Assert
//            await function.ProcessMessageAsync(new S3Event {
//                Records = new List<S3EventNotification.S3EventNotificationRecord> {
//                    new S3EventNotification.S3EventNotificationRecord {
//                        S3 = new S3EventNotification.S3Entity {
//                            Bucket = new S3EventNotification.S3BucketEntity {
//                                Name = "foo"
//                            },
//                            Object = new S3EventNotification.S3ObjectEntity {
//                                Key = "bar"
//                            }
//                        }
//                    }
//                }
//            }, new TestLambdaContext());
//        }
    }
}

//STOPPED HERE finish test above
//
//{
//BucketName = "foo",
//KeyName = "bar",
//NewSongs = new List<SongCsvModel> {
//    new SongCsvModel {
//        Artist = "Foo-Artist",
//        SearchArtist = "foo-artist",
//        SongNumber = "123",
//        Title = "Foo-Title",
//        SearchTitle = "foo-title"
//    }
//},
//OldSongs = new List<SongCsvModel> {
//    new SongCsvModel {
//        Artist = "Foo-Artist2",
//        SearchArtist = "foo-artist2",
//        SongNumber = "124",
//        Title = "Foo-Title2",
//        SearchTitle = "foo-title2"
//    }
//},
//}