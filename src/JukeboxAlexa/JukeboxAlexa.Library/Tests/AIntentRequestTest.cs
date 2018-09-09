using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using JukeboxAlexa.Library.Model;
using JukeboxAlexa.Library.TestFixtures;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace JukeboxAlexa.Library.Tests {
    public class AIntentRequestTest {
                    
        [Fact]
        public static void Abstract_intent_request__generate_sqs_request__valid() {
        
            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            var sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var aIntentRequestFixture = new AIntentRequestFixture(provider.Object, sqsClient.Object);
        
            // Act
            var response = aIntentRequestFixture.GenerateJukeboxSqsRequest("SpeakerRequest", "Turning the speaker on", "on");
        
            // Assert
            Assert.Equal("Turning the speaker on", response.MessageBody); 
            Assert.Equal("SpeakerRequest", response.RequestType); 
            Assert.Equal("on", response.Key);
        }
    
        [Fact]
        public static void Abstract_intent_request__generate_sqs_request__invalid() {
        
            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            var sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var aIntentRequestFixture = new AIntentRequestFixtureNotValidRequest(provider.Object, sqsClient.Object);
        
            // Act
            var response = aIntentRequestFixture.GenerateJukeboxSqsRequest("SpeakerRequest", "foo-bar", "foo-bar");
        
            // Assert
            Assert.Null(response); 
        }
        
        [Fact]
        public static async Task Abstract_intent_request__send_sqs_response__valid() {

            // Arrange
            var request = new JukeboxSqsRequest {
                Key = "123",
                MessageBody = "foo-bar",
                RequestType = "bat-baz"
            };
            var sendMessageRequest = new SendMessageRequest {
                QueueUrl = "http://foo-bar",
                MessageGroupId = "bat-baz",
                MessageDeduplicationId = "foo-date",
                MessageBody = JsonConvert.SerializeObject(request)
            };
            var provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            provider.Setup(x => x.DateNow()).Returns("foo-date");
            var sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            sqsClient.Setup(x => x.SendMessageAsync(sendMessageRequest, new CancellationToken()));
            var aIntentRequestFixture = new AIntentRequestFixture(provider.Object, sqsClient.Object);

            // Act
            await aIntentRequestFixture.SendSqsRequest(request, "bat-baz");

            // Assert
            provider.Verify(x => x.DateNow(), Times.Once);
            sqsClient.Verify(x => x.SendMessageAsync(
                It.Is<SendMessageRequest>(y => 
                    y.MessageBody == sendMessageRequest.MessageBody &&
                    y.MessageDeduplicationId == sendMessageRequest.MessageDeduplicationId &&
                    y.MessageGroupId == sendMessageRequest.MessageGroupId &&
                    y.QueueUrl == sendMessageRequest.QueueUrl
                ),
                new CancellationToken()
            ), Times.Once);
        }
        
        [Fact]
        public static async Task Abstract_intent_request__send_sqs_response__invalid() {

            // Arrange
            var request = new JukeboxSqsRequest {
                Key = "123",
                MessageBody = "foo-bar",
                RequestType = "bat-baz"
            };
            var sendMessageRequest = new SendMessageRequest {
                QueueUrl = "http://foo-bar",
                MessageGroupId = "foooooo",
                MessageDeduplicationId = "foo-date",
                MessageBody = JsonConvert.SerializeObject(request)
            };
            var provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            provider.Setup(x => x.DateNow()).Returns("foo-date");
            var sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            sqsClient.Setup(x => x.SendMessageAsync(sendMessageRequest, new CancellationToken()));
            var aIntentRequestFixture = new AIntentRequestFixture(provider.Object, sqsClient.Object);

            // Act
            await aIntentRequestFixture.SendSqsRequest(request, "bat-baz");

            // Assert
            provider.Verify(x => x.DateNow(), Times.Once);
            sqsClient.Verify(x => x.SendMessageAsync(
                It.Is<SendMessageRequest>(y => 
                    y.MessageBody == sendMessageRequest.MessageBody &&
                    y.MessageDeduplicationId == sendMessageRequest.MessageDeduplicationId &&
                    y.MessageGroupId == sendMessageRequest.MessageGroupId &&
                    y.QueueUrl == sendMessageRequest.QueueUrl
                ),
                new CancellationToken()
            ), Times.Never);
        }
    }
}