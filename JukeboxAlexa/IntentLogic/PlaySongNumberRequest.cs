using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library;
using JukeboxAlexa.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.IntentLogic
{
    public class PlaySongNumberRequest
    {
        //--- Methods ---
        public LookupResultModel.SongLookup ProcessRequest(string songNumberRequested)
        {
            Console.WriteLine($"*** INFO: PlaySongNumberRequest: {songNumberRequested}");
            LookupResultModel.SongLookup songLookup = new LookupResultModel.SongLookup
            {
                Request = new SongModel.Song
                {
                    TrackNumber = songNumberRequested
                },
                Response = new List<SongModel.Song>()
            };
            var jukeboxDynamoDb = new JukeboxDynamoDb();
            var foundSongsList = jukeboxDynamoDb.FindSongsByNumber(songLookup.Request.TrackNumber);
            
            // Handle no song returned.
            if (foundSongsList.Count == 0)
            {
                Console.WriteLine($"*** WARNING: No song found for {songLookup.Request.TrackNumber}");
                songLookup.SpeechText = $"No song found for {songLookup.Request.TrackNumber}";
            }
            
            // Handle more than one song returned. (i.e. same song title different artist.)
            if (foundSongsList.Count > 1)
            {
                // TODO List artists and list in speech text
                Console.WriteLine($"*** WARNING: More than one song found for {songLookup.Request.TrackNumber} - {JsonConvert.SerializeObject(foundSongsList)}");
                songLookup.SpeechText = $"More than one song found for {songLookup.Request.TrackNumber}";
            }

            // problem was found return
            if (foundSongsList.Count != 1) return songLookup;
            
            // process as normal
            Console.WriteLine($"*** INFO: Found song {songLookup.Request.TrackNumber}");
            songLookup.Response.Add(foundSongsList[0]);
            songLookup.SpeechText = $"Sending song number {foundSongsList[0].TrackNumber}, {foundSongsList[0].Title}, to the jukebox.";
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
