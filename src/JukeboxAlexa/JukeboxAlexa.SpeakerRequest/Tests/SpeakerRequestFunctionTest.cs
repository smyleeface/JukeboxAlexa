using System.Collections.Generic;
using Alexa.NET.Request;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.SQS;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using MindTouch.LambdaSharp;
using MindTouch.LambdaSharp.ConfigSource;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace JukeboxAlexa.SpeakerRequest.Tests {
    public class SpeakerRequestFunctionTest {

        [Fact]
        public async void Speaker_request__function() {

            // Arrange
            var intentRequest = new CustomSkillRequest {
                DialogState = "STARTED",
                Intent = new Intent {
                    Name = "SpeakerRequest",
                    ConfirmationStatus = "NONE",
                    Slots = new Dictionary<string, Slot> {
                        {
                            "Options", new Slot {
                                Name = "Options",
                                Value = "on"
                            }
                        }
                    }
                },
                Type = "IntentRequest"
            };
            Mock<ICommonDependencyProvider> provider = new Mock<ICommonDependencyProvider>(MockBehavior.Strict);
            Mock<IAmazonSQS> sqsClient = new Mock<IAmazonSQS>(MockBehavior.Strict);
            var body = new Dictionary<string, string>();
            var apiGatewayRequest = new APIGatewayProxyRequest {
                Body = JsonConvert.SerializeObject(intentRequest)
            };
            var function = new Function {
                speakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foobar")
            };

            // Act
            var result = await function.HandleRequestAsync(apiGatewayRequest, new TestLambdaContext());

            // Assert
            // TOOD: get the apigateway request & assert result
            Assert.Equal(200, result.StatusCode);
        }
        
        [Fact]
        public async void Speaker_request__init() {

            // Arrange
            var function = new Function();

            // Act
            var config = new LambdaConfig(new EmptyLambdaConfigSource());
            await function.InitializeAsync(config);

            // Assert
            Assert.IsType<SpeakerRequest>(function.speakerRequest);
        }
        
    }
}
