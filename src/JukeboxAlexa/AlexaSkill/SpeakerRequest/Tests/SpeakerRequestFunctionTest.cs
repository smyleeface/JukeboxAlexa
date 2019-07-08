using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SQS;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using LambdaSharp;
using LambdaSharp.ConfigSource;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace JukeboxAlexa.SpeakerRequest.Tests {
    public class SpeakerRequestFunctionTest {

        [Fact]
        public static async Task Speaker_request__function() {

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
            var apiGatewayRequest = new APIGatewayProxyRequest {
                Body = JsonConvert.SerializeObject(intentRequest)
            };
            var function = new Function {
                SpeakerRequest = new SpeakerRequest(provider.Object, sqsClient.Object, "foobar")
            };

            // Act
            var result = await function.ProcessProxyRequestAsync(apiGatewayRequest);

            // Assert
            Assert.Equal(200, result.StatusCode);
        }
        
        [Fact]
        public static async Task Speaker_request__init() {

            // Arrange
            var function = new Function();

            // Act
            var config = new LambdaConfig(new LambdaDictionarySource(new List<KeyValuePair<string, string>> {
               new KeyValuePair<string, string>("/SqsSongQueue", "foobar")
            }));
            await function.InitializeAsync(config);

            // Assert
            Assert.IsType<SpeakerRequest>(function.SpeakerRequest);
        }
        
    }
}
