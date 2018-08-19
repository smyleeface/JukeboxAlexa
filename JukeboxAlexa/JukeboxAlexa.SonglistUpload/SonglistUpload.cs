using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Castle.Core.Internal;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.SonglistUpload {
    public class SonglistUpload {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider dynamodbProvider;
        public readonly IS3DependencyProvider s3Provider;
        public string bucketName;
        public string keyName;
        public IEnumerable<SongCsvModel> newSongs;
        public IEnumerable<SongCsvModel> oldSongs;
        public IEnumerable<SongCsvModel> songsToAdd;
        public IEnumerable<SongCsvModel> songsToDelete;

        //--- Constructor ---
        public SonglistUpload(IDynamodbDependencyProvider awsDynmodbProvider, IS3DependencyProvider awsS3Provider) {
            dynamodbProvider = awsDynmodbProvider;
            s3Provider = awsS3Provider;
        }

        //--- Methods ---
        public async Task HandleRequest(string s3BucketName, string s3KeyName) {
            bucketName = s3BucketName;
            keyName = s3KeyName;
            await ReadNewSongs();
            await ReadOldSongs();
            FilterSongsToAdd();
            FilterSongsToDelete();
            await DeleteSongsFromDatabase();
            await AddSongsToDatabase();
            UpdateSummary();
        }

        public async Task ReadNewSongs() {
            var theseNewSongs = new List<SongCsvModel>();
            var getObjectResponse = await s3Provider.GetSongsFromS3UploadAsync(bucketName, keyName);
            var songRows = getObjectResponse.Split('\n');
            foreach (var songRow in songRows) {
                if (songRow.IsNullOrEmpty()) continue;
                var columns = songRow.Split(',');
                var parseResult = Int32.TryParse(columns[0], out var songNumber);
                if (!parseResult || columns[0].Length <= 0 || columns[3].Length <= 0) continue;
                var song = new SongCsvModel {
                    Artist = columns[4],
                    Number = $"{columns[0]}{columns[1]}",
                    Title = columns[3],
                    SearchArtist = columns[4].ToLower(),
                    SearchTitle = columns[3].ToLower()
                };
                theseNewSongs.Add(song);
            }
            newSongs = theseNewSongs;
        }

        public async Task ReadOldSongs() {
            var theseExsitingSongs = new List<SongCsvModel>();
            var rows = await dynamodbProvider.DynamoDbScanAsync();
            foreach (var item in rows.Items) {
                if (item.TryGetValue("Artist", out var artist) && item.TryGetValue("Number", out var number) && item.TryGetValue("Title", out var title) && item.TryGetValue("SearchTitle", out var searchTitle) && item.TryGetValue("SearchArtist", out var searchArtist)) {
                    var song = new SongCsvModel {
                        Artist = artist.S,
                        Number = number.S,
                        SearchArtist = searchArtist.S,
                        SearchTitle = searchTitle.S,
                        Title = title.S
                    };
                    theseExsitingSongs.Add(song);
                }
                oldSongs = theseExsitingSongs;
            }
        }

        public void FilterSongsToAdd() {
            var newSongsToAdd = new List<SongCsvModel>();
            foreach (var newSong in newSongs.ToList()) {
                var found = false;
                foreach (var oldSong in oldSongs.ToList()) {
                    if (newSong.Number == oldSong.Number && newSong.Artist == oldSong.Artist && newSong.Title == oldSong.Title) {
                        found = true;
                    }
                }
                if (!found) {
                    newSongsToAdd.Add(newSong);
                }
            }
            songsToAdd = newSongsToAdd;
        }

        public void FilterSongsToDelete() {
            var oldSongsToDelete = new List<SongCsvModel>();
            foreach (var oldSong in oldSongs) {
                var found = false;
                foreach (var newSong in newSongs) {
                    if (newSong.Number == oldSong.Number && newSong.Artist == oldSong.Artist && newSong.Title == oldSong.Title) {
                        found = true;
                    }
                }
                if (!found) {
                    oldSongsToDelete.Add(oldSong);
                }
            }
            songsToDelete = oldSongsToDelete;
        }

        public async Task DeleteSongsFromDatabase() {
            var batchDynamodbList = new List<List<WriteRequest>>();
            var dynamodbValues = new List<WriteRequest>();
            var batchCounter = 0;
            foreach (var deleteSong in songsToDelete) {
                var dbRecord = new WriteRequest {
                    DeleteRequest = new DeleteRequest {
                        Key = new Dictionary<string, AttributeValue> {
                            { "title", new AttributeValue {
                                S = deleteSong.Title
                            }},
                            { "artist", new AttributeValue {
                                S = deleteSong.Artist
                            }}
                        }
                    }
                };
                dynamodbValues.Add(dbRecord);
                batchCounter += 1;
                if (batchCounter % 25 == 0 || batchCounter == songsToDelete.Count())
                    batchDynamodbList.Add(dynamodbValues);
                    dynamodbValues = new List<WriteRequest>();
            }
            foreach (var dynamodbList in batchDynamodbList) {
                await dynamodbProvider.DynamoDbBatchWriteItemAsync(dynamodbList);
            }
        }

        public async Task AddSongsToDatabase() {
            var batchDynamodbList = new List<List<WriteRequest>>();
            var dynamodbValues = new List<WriteRequest>();
            var batchCounter = 0;
            foreach (var addSong in songsToAdd) {
                var dbRecord = new WriteRequest {
                    PutRequest = new PutRequest {
                        Item = new Dictionary<string, AttributeValue> {
                            { "title", new AttributeValue {
                                S = addSong.Title
                            }},
                            { "artist", new AttributeValue {
                                S = addSong.Artist
                            }},
                            { "number", new AttributeValue {
                                S = addSong.Number
                            }},
                            { "search_title", new AttributeValue {
                                S = addSong.SearchTitle
                            }},
                            { "search_artist", new AttributeValue {
                                S = addSong.SearchTitle
                            }}
                        }
                    }
                };
                dynamodbValues.Add(dbRecord);
                batchCounter += 1;
                if (batchCounter % 25 == 0 || batchCounter == songsToAdd.Count())
                    batchDynamodbList.Add(dynamodbValues);
                dynamodbValues = new List<WriteRequest>();
            }
            foreach (var dynamodbList in batchDynamodbList) {
                await dynamodbProvider.DynamoDbBatchWriteItemAsync(dynamodbList);
            }
        }
        
        public void UpdateSummary() {
            LambdaLogger.Log($"Added {songsToAdd.Count()} Songs");    
            LambdaLogger.Log($"Deleted {songsToDelete.Count()} Songs");    
        }
    }
}