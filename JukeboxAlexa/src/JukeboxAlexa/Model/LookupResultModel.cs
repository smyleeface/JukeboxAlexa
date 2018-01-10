using System.Collections.Generic;
using Alexa.NET.Request;
using Newtonsoft.Json;
using JukeboxAlexa.Model;

namespace JukeboxAlexa.Model

{
    public class LookupResultModel
    {
        public class SongLookup
        {
            [JsonProperty("intentType")]
            public string IntentType { get; set; }

            [JsonProperty("request")]
            public SongModel.Song Request { get; set; }

            [JsonProperty("response")]
            public List<SongModel.Song> Response { get; set; }
            
            [JsonProperty("speechText")]
            public string SpeechText { get; set; }
                        
            [JsonProperty("repromptBody")]
            public Alexa.NET.Response.Reprompt RepromptBody { get; set; }
            
            [JsonProperty("snsResponse")]
            public SnsMessageBody.Response SnsResponse { get; set; }

        }
    }
}