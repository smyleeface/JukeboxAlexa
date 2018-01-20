using System;
using Alexa.NET;
using Alexa.NET.Response;

namespace JukeboxAlexa.Library
{
    public class JukeboxAlexaResponse
    {
        public static SkillResponse GenerateResponseCard(string speechResult)
        {
            var cardTitle = "Jukebox - Song Request";
            var speechOutput = $"<speak>{speechResult}<break strength=\"x-strong\"/>I hope you have a good day.</speak>";
            Console.WriteLine($"*** INFO: speechOutput -> {speechOutput}");
            var speech = new SsmlOutputSpeech
            {
                Ssml = speechOutput
            };
            return ResponseBuilder.TellWithCard(speech, cardTitle, speechOutput);
        }
        
        public static void SendSongToJukeboxQueue()
        {
//                        if (foundSong)
//                        {
//                            // TODO: find song in song list, return songKey, generate speechOutput
//                            speechText = $"Sending song number {songKeyRequest}, {songTitle}, to the jukebox.";
//
////                             sendQueueMessage({
////                                request_type: 'GetSongRequested',
////                                parameters: {
////                                    key: songKey,
////                                    message_body: speechOutput
////                                }
////                            }
////                            var speechOutput = "Sorry, something went wrong.";
//                        }
        }
    }
}