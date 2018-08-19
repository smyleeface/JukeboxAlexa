using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Model {
    public class SongCsvModel {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("artist")]
        public string Artist;

        [JsonProperty("number")]
        public string Number;

        [JsonProperty("search_title")]
        public string SearchTitle;

        [JsonProperty("search_artist")]
        public string SearchArtist;
    }
}