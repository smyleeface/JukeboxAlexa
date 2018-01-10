using System.Collections.Generic;
using Newtonsoft.Json;
using JukeboxAlexa.Model;

namespace JukeboxAlexa.Model

{
    public class SnsMessageBody
    {
        public class Response
        {
            [JsonProperty("request_type")]
            public string RequestType { get; set; }

            [JsonProperty("parameters")]
            public Parameters Parameters { get; set; }

        }
        
        public class Parameters
        {
            [JsonProperty("key")]
            public string Key { get; set; }
            
            [JsonProperty("message_body")]
            public string MessageBody { get; set; }
        }

    }
}