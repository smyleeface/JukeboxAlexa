using System;
using System.Collections.Generic;
using JukeboxAlexa.Model;
using JukeboxAlexa.Library;
using Newtonsoft.Json;

namespace JukeboxAlexa.IntentLogic
{
    public class PlaySongArtistRequest
    {
        //--- Methods ---
        public LookupResultModel.SongLookup ProcessRequest(string songRequested, string artistRequested)
        {
            Console.WriteLine($"*** INFO: PlaySongArtistRequest: {songRequested}");
            LookupResultModel.SongLookup songLookup = new LookupResultModel.SongLookup
            {
                Request = new SongModel.Song
                {
                    Title = songRequested,
                    Artist = artistRequested
                },
                Response = new List<SongModel.Song>()
            };
            var jukeboxDynamoDb = new JukeboxDynamoDb();
            var foundSongsList = jukeboxDynamoDb.FindSongsByTitleArtist(songLookup.Request.Title, songLookup.Request.Artist);

            // Handle no song returned.
            if (foundSongsList.Count == 0)
            {
                Console.WriteLine($"*** WARNING: No song found for {songLookup.Request.Title} by {songLookup.Request.Artist}");
                songLookup.SpeechText = $"No song found for {songLookup.Request.Title} by {songLookup.Request.Artist}";
            }
            
            // Handle more than one song returned. (i.e. same song title different artist.)
            if (foundSongsList.Count > 1)
            {
                // TODO List artists and list in speech text
                Console.WriteLine($"*** WARNING: More than one song found for {songLookup.Request.Title} by {songLookup.Request.Artist} - {JsonConvert.SerializeObject(foundSongsList)}");
                songLookup.SpeechText = $"More than one song found for {songLookup.Request.Title} by {songLookup.Request.Artist}";
            }

            // problem was found return
            if (foundSongsList.Count != 1) return songLookup;
            
            // process as normal
            Console.WriteLine($"*** INFO: Found song {songLookup.Request.Title} by {songLookup.Request.Artist}");
            songLookup.Response.Add(foundSongsList[0]);
            songLookup.SpeechText = $"Sending song number {foundSongsList[0].TrackNumber}, {foundSongsList[0].Title} by {songLookup.Request.Artist}, to the jukebox.";
            songLookup.SnsResponse = new SnsMessageBody.Response
            {
                Parameters = new SnsMessageBody.Parameters
                {
                    Key = foundSongsList[0].TrackNumber,
                    MessageBody = songLookup.SpeechText
                }
            };
            return songLookup;
        }
    }
}