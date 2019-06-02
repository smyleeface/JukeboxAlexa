using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Model {
    public class JukeboxSqsRequest {

            [JsonProperty("request_type")]
            public string RequestType;

            [JsonProperty("key")]
            public string Key;

            [JsonProperty("message_body")]
            public string MessageBody;
    }
}
