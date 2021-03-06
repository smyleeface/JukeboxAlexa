﻿using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Model

{
    public class SongModel
    {
        public class Song
        {
            [JsonProperty("song_number")]
            public string SongNumber { get; set; }
        
            [JsonProperty("title")]
            public string Title { get; set; }
        
            [JsonProperty("artist")]
            public string Artist { get; set; }
        
        }
        
        public class SongCache
        {
            [JsonProperty("count")]
            public string Count { get; set; }
        
            [JsonProperty("song")]
            public Song Song { get; set; }
        
        }
    }
}