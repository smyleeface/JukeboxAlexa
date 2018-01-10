using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library
{
    public class JukeboxDynamoDb
    {
        
        //--- Fields ---
        private static AmazonDynamoDBClient _dynamoClient;
        private static string _tableName;
        private static string _indexTrackNumber;
        private static string _indexSearchTitle;
        private static string _indexSearchTitleArtist;

        //--- Constructors ---
        public JukeboxDynamoDb()
        {
            _dynamoClient = new AmazonDynamoDBClient();
            _tableName = "JukeboxSongs";
            _indexTrackNumber = "track_number-index";
            _indexSearchTitle = "search_title-index";
            _indexSearchTitleArtist = "search_title_artist-index";
        }
        
        //--- Methods ---
        public List<SongModel.Song> FindSongsByTitleArtist(string title, string artist)
        {
            Console.WriteLine($"*** INFO: FindSongsByTitleArtist for `{title}` and `{artist}");
            var queryRequest = QueryRequestTitleArtist(title, artist);
            var queryResponse = _dynamoClient.QueryAsync(queryRequest).Result;
            Console.WriteLine($"*** INFO: queryResponse `{JsonConvert.SerializeObject(queryResponse)}`");
            var parsedSongList = ParseSongsFromDatabaseResponse(queryResponse);
            return parsedSongList;
           
        }
        
        public List<SongModel.Song> FindSongsByTitle(string title)
        {
            Console.WriteLine($"*** INFO: FindSongsByTitle for `{title}`");
            var queryRequest = QueryRequestTitle(title);
            var queryResponse = _dynamoClient.QueryAsync(queryRequest).Result;
            Console.WriteLine($"*** INFO: queryResponse `{JsonConvert.SerializeObject(queryResponse)}`");
            var parsedSongList = ParseSongsFromDatabaseResponse(queryResponse);
            return parsedSongList;
           
        }

        public List<SongModel.Song> FindSongsByNumber(string trackNumber)
        {
            Console.WriteLine($"*** INFO: FindSongsByNumber for `{trackNumber}`");
            var queryRequest = QueryRequestNumber(trackNumber);                    
            var queryResponse = _dynamoClient.QueryAsync(queryRequest).Result;
            Console.WriteLine($"*** INFO: queryResponse `{JsonConvert.SerializeObject(queryResponse)}`");
            var parsedSongList = ParseSongsFromDatabaseResponse(queryResponse);
            return parsedSongList; 
        }
        
        private QueryRequest QueryRequestTitleArtist(string title, string artist)
        {
            return new QueryRequest{
                TableName = _tableName,
                IndexName = _indexSearchTitleArtist,
                KeyConditionExpression = "search_title = :v_song AND search_artist = :v_artist",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_song", new AttributeValue { S = title.ToLower() }},
                    {":v_artist", new AttributeValue { S = artist.ToLower() }}
                }
            };
        }

        private QueryRequest QueryRequestTitle(string title)
        {
            return new QueryRequest{
                TableName = _tableName,
                IndexName = _indexSearchTitle,
                KeyConditionExpression = "search_title = :v_song",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_song", new AttributeValue { S = title.ToLower() }}}
            };
        }
        
        private QueryRequest QueryRequestNumber(string songNumber)
        {
            return new QueryRequest{
                TableName = _tableName,
                IndexName = _indexTrackNumber,
                KeyConditionExpression = "track_number = :v_number",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_number", new AttributeValue { S = songNumber }}}
            };
        }
        
        private static List<SongModel.Song> ParseSongsFromDatabaseResponse(QueryResponse queryResponse)
        {
            var parsedSongList = new List<SongModel.Song>();
            foreach (Dictionary<string, AttributeValue> item in queryResponse.Items)
            {
                var song = new SongModel.Song();
                foreach (KeyValuePair<string, AttributeValue> attribute in item)
                {
                    var attributeValue = attribute.Value.S;
                    switch (attribute.Key)
                    {
                        case "title":
                            song.Title = attributeValue;
                            break;
                            
                        case "artist":
                            song.Artist = attributeValue;
                            break;
                        
                        case "track_number":
                            song.TrackNumber = attributeValue;
                            break;
                    }
                }
                parsedSongList.Add(song);
            }
            Console.WriteLine($"*** INFO: {JsonConvert.SerializeObject(parsedSongList)}");
            return parsedSongList;
        }
    }
}