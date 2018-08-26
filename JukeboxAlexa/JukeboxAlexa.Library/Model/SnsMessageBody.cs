using System.Collections.Generic;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Model

{
    public class SnsMessageBody
    {
        public class Response
        {
            [JsonProperty("request_type")]
            public string RequestType { get; set; }

            [JsonProperty("key")]
            public string Key { get; set; }
            
            [JsonProperty("message_body")]
            public string MessageBody { get; set; }
        }

    }
}