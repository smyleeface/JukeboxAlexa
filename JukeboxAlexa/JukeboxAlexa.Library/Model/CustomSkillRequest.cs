using System;
using System.Collections.Generic;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Model {
    public class CustomSkillRequest {
        
        [JsonProperty("type")]
        public string Type;
        
        [JsonProperty("dialog_state")]
        public string DialogState;
        
        [JsonProperty("intent")]
        public Intent Intent;

    }
}