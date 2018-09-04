using System.Collections.Generic;
using Alexa.NET;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library {
    public static class AlexaResponse {
        public static SkillResponse Generate(string responseType, string message) {
            switch (responseType.ToLower()) {
                case "ask": {
                    return GenerateAlexaAskResponse(message, new Dictionary<string, object>(), true);
                }
                case "tellwithcard": {
                    return GenerateAlexaTellWithCardResponse(message, "");
                }
                case "tell": {
                    return GenerateAlexaTellResponse(message);
                }
            }
            return GenerateAlexaTellResponse(message);
        }

        public static SkillResponse GenerateAlexaTellResponse(string message) {
            var finalResponse = ResponseBuilder.Tell(message);
            LambdaLogger.Log($"*** INFO: Alexa final response: {JsonConvert.SerializeObject(finalResponse)}");
            return finalResponse;
        }

        public static SkillResponse GenerateAlexaAskResponse(string message, Dictionary<string, object> sessionAttributes, bool shouldEndSession) {
            var repromptMessage = GenerateRepromptMessage(message);
            var finalResponse = ResponseBuilder.Ask(message, repromptMessage);
            finalResponse.SessionAttributes = sessionAttributes;
            finalResponse.Response.ShouldEndSession = shouldEndSession;
            LambdaLogger.Log($"*** INFO: Alexa final response: {JsonConvert.SerializeObject(finalResponse)}");
            return finalResponse;
        }

        public static SkillResponse GenerateAlexaTellWithCardResponse(string message, string requestType) {
            var cardTitle = "Jukebox Request";
            var speechOutput = $"<speak>{message}<break strength=\"x-strong\"/>I hope you have a good day.</speak>";
            var speech = new SsmlOutputSpeech {
                Ssml = speechOutput
            };
            var finalResponse = ResponseBuilder.TellWithCard(speech, cardTitle, speechOutput);
            LambdaLogger.Log($"*** INFO: speechOutput: {JsonConvert.SerializeObject(finalResponse)}");
            return finalResponse;
        }

        public static Reprompt GenerateRepromptMessage(string message) {
            var speech = new SsmlOutputSpeech {
                Ssml = message
            };
            return new Reprompt {
                OutputSpeech = speech
            };
        }
    }
}