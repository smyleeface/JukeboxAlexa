using System.Collections.Generic;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Model {
    public class CustomSkillResponse {
        
        [JsonProperty("message")]
        public string Message;
        
        [JsonProperty("request_type")]
        public string RequestType;        
        
        [JsonProperty("session_attributes")]
        public IDictionary<string, object> SessionAttributes;
            
        [JsonProperty("should_end_session")]
        public bool ShouldEndSession;
    }
}