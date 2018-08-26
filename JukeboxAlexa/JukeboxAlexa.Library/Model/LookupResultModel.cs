using System.Collections.Generic;
using Alexa.NET.Request;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Model

{
    public class LookupResultModel
    {
        public class SongLookup
        {
            [JsonProperty("intentType")]
            public string IntentType { get; set; }

            [JsonProperty("songRequest")]
            public SongModel.Song SongRequest { get; set; }

            [JsonProperty("songsFound")]
            public List<SongModel.Song> SongsFound { get; set; }
            
            [JsonProperty("speechText")]
            public string SpeechText { get; set; }
                        
            [JsonProperty("repromptBody")]
            public Alexa.NET.Response.Reprompt RepromptBody { get; set; }

        }
    }
}