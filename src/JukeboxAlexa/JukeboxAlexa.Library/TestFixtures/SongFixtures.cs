using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.Model;

namespace JukeboxAlexa.Library.TestFixtures {
    public class SongFixtures {

        public string TableName = "JukeboxSongs";
        public string IndexNameSearchTitle = "search_title-index";
        public string IndexNameSearchTitleArtist = "search_title_artist-index";

        public SongModel.Song Song1 = new SongModel.Song {
            SongNumber = "328",
            Artist = "Mumford & Sons",
            Title = "I Will Wait"
        };
        
        public Dictionary<string, AttributeValue> SongAttribute1 = new Dictionary<string, AttributeValue> {
            {"track_number", new AttributeValue("328")},
            {"artist", new AttributeValue("Mumford & Sons")},
            {"search_artist", new AttributeValue("mumford & sons")},
            {"search_title", new AttributeValue("i will wait")},
            {"title", new AttributeValue("I Will Wait")}
        };
        
        public SongModel.Song Song2 = new SongModel.Song {
            SongNumber = "123",
            Artist = "Lionel Ritche",
            Title = "Hello"
        };
        
        public Dictionary<string, AttributeValue> SongAttribute2 = new Dictionary<string, AttributeValue> {
            {"track_number", new AttributeValue("123")},
            {"artist", new AttributeValue("Lionel Ritche")},
            {"search_artist", new AttributeValue("lionel ritche")},
            {"search_title", new AttributeValue("hello")},
            {"title", new AttributeValue("Hello")}
        };        
        
        public SongModel.Song Song3 = new SongModel.Song {
            SongNumber = "456",
            Artist = "Lionel Ritche",
            Title = "I Just Called"
        };
        
        public Dictionary<string, AttributeValue> SongAttribute3 = new Dictionary<string, AttributeValue> {
            {"track_number", new AttributeValue("456")},
            {"artist", new AttributeValue("Lionel Ritche")},
            {"search_artist", new AttributeValue("lionel ritche")},
            {"search_title", new AttributeValue("i just called")},
            {"title", new AttributeValue("I Just Called")}
        };        

        public SongModel.Song Song4 = new SongModel.Song {
            SongNumber = "789",
            Artist = "Adele",
            Title = "Hello"
        };

        public Dictionary<string, AttributeValue> SongAttribute4 = new Dictionary<string, AttributeValue> {
            {"track_number", new AttributeValue("789")},
            {"artist", new AttributeValue("Adele")},
            {"search_artist", new AttributeValue("adele")},
            {"search_title", new AttributeValue("hello")},
            {"title", new AttributeValue("Hello")}
        };

        public IEnumerable<IDictionary<string, AttributeValue>> Songs;

        public SongFixtures() {
            Songs = new List<Dictionary<string, AttributeValue>> {
                SongAttribute1,
                SongAttribute2,
                SongAttribute3,
                SongAttribute4
            };
        }
    }
}