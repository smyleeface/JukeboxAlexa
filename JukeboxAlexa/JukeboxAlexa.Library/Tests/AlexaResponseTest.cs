using System.Collections.Generic;
using Alexa.NET;
using Alexa.NET.Response;
using Amazon.SQS;
using JukeboxAlexa.Library.TestFixture;
using Moq;
using Xunit;

namespace JukeboxAlexa.Library.Tests {
    public class AlexaResponseTest {
                    
        [Fact]
        public void Abstract_intent_request__generate_alexa_ask_response() {
        
            // Arrange
            var speech = new SsmlOutputSpeech {
                Ssml = "foo-bar"
            };
            var repromptMessage = new Reprompt {
                OutputSpeech = speech
            };
            var expectedResponse = ResponseBuilder.Ask("foo-bar", repromptMessage);
            expectedResponse.SessionAttributes = new Dictionary<string, object>();
            expectedResponse.Response.ShouldEndSession = true;
            
            // Act
            var response = AlexaResponse.GenerateAlexaAskResponse("foo-bar", new Dictionary<string, object>(), true);
        
            // Assert 
            var expectedOutputSpeech = (PlainTextOutputSpeech) expectedResponse.Response.OutputSpeech;
            var responseOutputSpeech = (PlainTextOutputSpeech) response.Response.OutputSpeech;
            Assert.Equal(expectedOutputSpeech.Text, responseOutputSpeech.Text); 
            Assert.Equal(expectedOutputSpeech.Type, responseOutputSpeech.Type); 
            Assert.Equal(expectedResponse.SessionAttributes, response.SessionAttributes);  
        }
                    
        [Fact]
        public void Abstract_intent_request__generate_alexa_tell_with_card_response() {
        
            // Arrange
            var speechOutput = "<speak>foo-bar<break strength=\"x-strong\"/>I hope you have a good day.</speak>";
            var speech = new SsmlOutputSpeech {
                Ssml = speechOutput
            };
            var expectedResponse = ResponseBuilder.TellWithCard(speech, "Jukebox Request", speechOutput);    
        
            // Act
            var response = AlexaResponse.GenerateAlexaTellWithCardResponse("foo-bar", "bat-baz");

            // Assert
            var expectedOutputSpeech = (SsmlOutputSpeech) expectedResponse.Response.OutputSpeech;
            var responseOutputSpeech = (SsmlOutputSpeech) response.Response.OutputSpeech;
            Assert.Equal(expectedOutputSpeech.Ssml, responseOutputSpeech.Ssml); 
            Assert.Equal(expectedOutputSpeech.Type, responseOutputSpeech.Type); 
            Assert.Equal(expectedResponse.SessionAttributes, response.SessionAttributes);
        }
    }
}