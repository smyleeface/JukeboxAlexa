using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using JukeboxAlexa.Library;
using JukeboxAlexa.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JukeboxAlexa.IntentLogic
{
    public class FindSongRequested
    {
        
        public LookupResultModel.SongLookup ProcessRequest(string requestedPhrase)
        {    
            Console.WriteLine($"*** INFO: FindSongRequested: {requestedPhrase}");

            // initalize songLookup
            LookupResultModel.SongLookup songLookup = new LookupResultModel.SongLookup
            {
                Request = new SongModel.Song
                {
                    Title = requestedPhrase
                },
                Response = new List<SongModel.Song>()
            };

            // get the songs from the database
            var jukeboxCacheDynamoDb = new JukeboxCacheDynamoDb();
            var songsFromDatabaseCache = jukeboxCacheDynamoDb.FindWordsFromCache(requestedPhrase);
            Console.WriteLine($"**** INFO: foundSongs {JsonConvert.SerializeObject(songsFromDatabaseCache)}");

            var totalReturnedIndexCount = songsFromDatabaseCache.Count-1;
            for (var index = 0; index <= totalReturnedIndexCount; index++)
            {
                songsFromDatabaseCache = sortSongsFromDatabaseCache(songsFromDatabaseCache, index, totalReturnedIndexCount);
            }
            
            var maxSongsIndexCount = 2;
            if (totalReturnedIndexCount < maxSongsIndexCount)
            {
                maxSongsIndexCount = totalReturnedIndexCount;
            }

            var speechText = $"Found {songsFromDatabaseCache.Count} songs with the words {requestedPhrase}. Here are the top {maxSongsIndexCount + 1} choices: ";

            // create the speech prompt
            for (var i = 0; i <= maxSongsIndexCount; i++)
            {  
//                repromptMessage.Text += $"Song {songsFromDatabaseCache[i].Song.Title} by {songsFromDatabaseCache[i].Song.Artist} number {songsFromDatabaseCache[i].Song.TrackNumber}. ";
                speechText += $"Song {songsFromDatabaseCache[i].Song.Title} by {songsFromDatabaseCache[i].Song.Artist} number {songsFromDatabaseCache[i].Song.TrackNumber}. ";
            }
            
            // send it back
            Console.WriteLine($"**** INFO: speechText {speechText}");
            songLookup.SpeechText = speechText;
//            songLookup.RepromptBody = new Alexa.NET.Response.Reprompt {OutputSpeech = repromptMessage};
            return songLookup;
        }

        List<SongModel.SongCache> sortSongsFromDatabaseCache(List<SongModel.SongCache> songsFromDatabaseCacheSorted, int lastIndex, int totalIndex)
        {
            
            // return when we're at the end of the list
            if (lastIndex >= totalIndex || lastIndex <= 0) return songsFromDatabaseCacheSorted;

            var thisSongCache = songsFromDatabaseCacheSorted[lastIndex];
            int.TryParse(thisSongCache.Count, out int thisCounter);
            
            // get the next song in the list
            var nextSongCache = songsFromDatabaseCacheSorted[lastIndex + 1];
            int.TryParse(nextSongCache.Count, out int nextCounter);
            
            if (thisCounter > nextCounter)
            {
                // remove next song and put in this counters place
                songsFromDatabaseCacheSorted.RemoveAt(lastIndex + 1);
                songsFromDatabaseCacheSorted.Insert(lastIndex, nextSongCache);
                sortSongsFromDatabaseCache(songsFromDatabaseCacheSorted, lastIndex-1, totalIndex);
            }
            return songsFromDatabaseCacheSorted;
        }
    }
}
