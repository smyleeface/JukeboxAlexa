using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Amazon.SQS;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Moq;
using Xunit;

namespace JukeboxAlexa.SpeakerRequest.Tests {
    public class SpeakerRequestTest {

        [Fact]
        public void Speaker_request__get_song_info_requested__found() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var slots = new Dictionary<string, Slot> {
                {
                    "Options", new Slot {
                        Name = "Options",
                        Value = "on"
                    }
                }
            };
            var speakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue");

            // Act
            speakerRequest.GetSongInfoRequested(slots);

            // Assert
            Assert.Equal("on", speakerRequest.SpeakerAction);
        }
        
        [Fact]
        public void Speaker_request__get_song_info_requested__not_found() {

            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var slots = new Dictionary<string, Slot> {
                {
                    "FooBar", new Slot {
                        Name = "FooBar",
                        Value = "on"
                    }
                }
            };
            var speakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue");

            // Act
            speakerRequest.GetSongInfoRequested(slots);

            // Assert
            Assert.Null(speakerRequest.SpeakerAction);
        }

        [Fact]
        public void Speaker_request__is_valid_request__true() {
            
            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var speakerRequestOn = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue") {
                SpeakerAction = "on"
            };
            var speakerRequestOff = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue") {
                SpeakerAction = "off"
            };

            // Act
            var responseOn = speakerRequestOn.IsValidRequest();
            var responseOff = speakerRequestOff.IsValidRequest();
            
            // Assert
            Assert.True(responseOn);
            Assert.True(responseOff);
        }

        [Fact]
        public void Speaker_request__is_valid_request__false() {
            
            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var speakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue") {
                SpeakerAction = "foo-bar"
            };

            // Act
            var response = speakerRequest.IsValidRequest();
            
            // Assert
            Assert.False(response);
        }
        
        [Fact]
        public void Speaker_request__generate_message__valid() {
            
            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var speakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue") {
                SpeakerAction = "on"
            };

            // Act
            var response = speakerRequest.GenerateMessage();
            
            // Assert
            Assert.Contains("speaker on", response); 
        }        
        
        [Fact]
        public void Speaker_request__generate_message__invalid() {
            
            // Arrange
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var speakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue") {
                SpeakerAction = "foo-bar"
            };

            // Act
            var response = speakerRequest.GenerateMessage();
            
            // Assert
            Assert.Contains("do not understand", response); 
            Assert.Contains("foo-bar", response); 
        }
        
        [Fact]
        public async Task Speaker_request__handle_request() {
            
            // Arrange
            var customSkillRequest = new CustomSkillRequest {
                Intent = new Intent {
                    Name = "SpeakerRequest",
                    Slots = new Dictionary<string, Slot> {
                        {
                            "Options", new Slot {
                                Name = "Options",
                                Value = "on"
                            }
                        }
                    }
                },
                DialogState = "STARTED",
                Type = "SpeakerRequest"
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var speakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foo-bar-queue") {
                SpeakerAction = "on"
            };

            // Act
            var response = await speakerRequest.HandleRequest(customSkillRequest);
            
            // Assert
            Assert.Equal("Turning the jukebox speaker on", response.Message); 
        }
    }
}
