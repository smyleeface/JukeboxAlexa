using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Castle.Core.Internal;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.SonglistUpload {
    public class SonglistUpload {

        //--- Fields ---
        public readonly IDynamodbDependencyProvider DynamodbProvider;
        public readonly IS3DependencyProvider S3Provider;
        public string BucketName;
        public string KeyName;
        public IEnumerable<SongCsvModel> NewSongs;
        public IEnumerable<SongCsvModel> OldSongs;
        public IEnumerable<SongCsvModel> SongsToAdd;
        public IEnumerable<SongCsvModel> SongsToDelete;

        //--- Constructor ---
        public SonglistUpload(IDynamodbDependencyProvider awsDynmodbProvider, IS3DependencyProvider awsS3Provider) {
            DynamodbProvider = awsDynmodbProvider;
            S3Provider = awsS3Provider;
        }

        //--- Methods ---
        public async Task HandleRequest(string s3BucketName, string s3KeyName) {
            BucketName = s3BucketName;
            KeyName = s3KeyName;
            LambdaLogger.Log($"***INFO: bucket: {s3BucketName}; key: {KeyName}");
            await ReadNewSongs().ConfigureAwait(false);
            await ReadOldSongs().ConfigureAwait(false);
            FilterSongsToAdd();
            FilterSongsToDelete();
            await DeleteSongsFromDatabase().ConfigureAwait(false);
            await AddSongsToDatabase().ConfigureAwait(false);
            UpdateSummary();
        }

        public async Task ReadNewSongs() {
            var theseNewSongs = new List<SongCsvModel>();
            var getObjectResponse = await S3Provider.GetSongsFromS3UploadAsync(BucketName, KeyName);
            var songRows = getObjectResponse.Split('\n');
            LambdaLogger.Log($"***INFO: new songs from file (songRows): {JsonConvert.SerializeObject(songRows)}");
            foreach (var songRow in songRows) {
                if (songRow.IsNullOrEmpty()) {
                    continue;
                }
                var columns = songRow.Split(',');
                var parseResult = Int32.TryParse(columns[0], out var songNumber);
                if (!parseResult || columns[0].Length <= 0 || columns[2].Length <= 0) {
                    continue;
                }
                var song = new SongCsvModel {
                    Artist = columns[4],
                    SongNumber = $"{columns[0]}{columns[1]}",
                    Title = columns[3],
                    SearchArtist = columns[4].ToLower(),
                    SearchTitle = columns[3].ToLower()
                };
                theseNewSongs.Add(song);
            }
            LambdaLogger.Log($"***INFO: new songs filtered (theseNewSongs): {JsonConvert.SerializeObject(theseNewSongs)}");
            NewSongs = theseNewSongs;
        }

        public async Task ReadOldSongs() {
            var theseExsitingSongs = new List<SongCsvModel>();
            var rows = await DynamodbProvider.DynamoDbScanAsync();
            LambdaLogger.Log($"***INFO: old songs from database (rows.Items): {JsonConvert.SerializeObject(rows.Items)}");
            foreach (var item in rows.Items) {
                if (item.TryGetValue("artist", out var artist) && item.TryGetValue("song_number", out var number) && item.TryGetValue("title", out var title) && item.TryGetValue("search_title", out var searchTitle) && item.TryGetValue("search_artist", out var searchArtist)) {
                    var song = new SongCsvModel {
                        Artist = artist.S,
                        SongNumber = number.S,
                        SearchArtist = searchArtist.S,
                        SearchTitle = searchTitle.S,
                        Title = title.S
                    };
                    theseExsitingSongs.Add(song);
                }
            }
            LambdaLogger.Log($"***INFO: old songs filtered (theseExsitingSongs): {JsonConvert.SerializeObject(theseExsitingSongs)}");
            OldSongs = theseExsitingSongs;
        }

        public void FilterSongsToAdd() {
            var newSongsToAdd = new List<SongCsvModel>();
            foreach (var newSong in NewSongs) {
                var found = false;
                foreach (var oldSong in OldSongs) {
                    if (newSong.SongNumber == oldSong.SongNumber && newSong.Artist == oldSong.Artist && newSong.Title == oldSong.Title) {
                        found = true;
                    }
                }
                if (!found) {
                    newSongsToAdd.Add(newSong);
                }
            }
            LambdaLogger.Log($"***INFO: new songs marked for adding to database (newSongsToAdd): {JsonConvert.SerializeObject(newSongsToAdd)}");
            SongsToAdd = newSongsToAdd;
        }

        public void FilterSongsToDelete() {
            var oldSongsToDelete = new List<SongCsvModel>();
            foreach (var oldSong in OldSongs) {
                var found = false;
                foreach (var newSong in NewSongs) {
                    if (newSong.SongNumber == oldSong.SongNumber && newSong.Artist == oldSong.Artist && newSong.Title == oldSong.Title) {
                        found = true;
                    }
                }
                if (!found) {
                    oldSongsToDelete.Add(oldSong);
                }
            }
            LambdaLogger.Log($"***INFO: old songs marked for deleting from database (oldSongsToDelete): {JsonConvert.SerializeObject(oldSongsToDelete)}");
            SongsToDelete = oldSongsToDelete;
        }

        public async Task DeleteSongsFromDatabase() {
            var batchDynamodbList = new List<List<WriteRequest>>();
            var dynamodbValues = new List<WriteRequest>();
            var batchCounter = 0;
            foreach (var deleteSong in SongsToDelete) {
                var dbRecord = new WriteRequest {
                    DeleteRequest = new DeleteRequest {
                        Key = new Dictionary<string, AttributeValue> {
                            { "song_number", new AttributeValue {
                                S = deleteSong.SongNumber
                            }}
                        }
                    }
                };
                dynamodbValues.Add(dbRecord);
                batchCounter += 1;
                if (batchCounter % 25 == 0 || batchCounter == SongsToDelete.Count()) {
                    batchDynamodbList.Add(dynamodbValues);       
                    dynamodbValues = new List<WriteRequest>();
                }
            }
            foreach (var dynamodbList in batchDynamodbList) {
                LambdaLogger.Log($"***INFO: writing to delete from database (dynamodbList): {JsonConvert.SerializeObject(dynamodbList)}");
                await DynamodbProvider.DynamoDbBatchWriteItemAsync(dynamodbList);
            }
        }

        public async Task AddSongsToDatabase() {
            var batchDynamodbList = new List<List<WriteRequest>>();
            var dynamodbValues = new List<WriteRequest>();
            var batchCounter = 0;
            foreach (var addSong in SongsToAdd) {
                var putRequest = new PutRequest {
                    Item = new Dictionary<string, AttributeValue> {
                        {
                            "title", new AttributeValue {
                                S = addSong.Title
                            }
                        }, {
                            "artist", new AttributeValue {
                                S = addSong.Artist
                            }
                        }, {
                            "song_number", new AttributeValue {
                                S = addSong.SongNumber
                            }
                        }, {
                            "search_title", new AttributeValue {
                                S = addSong.SearchTitle
                            }
                        }, {
                            "search_artist", new AttributeValue {
                                S = addSong.SearchTitle
                            }
                        }
                    }
                };
                dynamodbValues.Add(new WriteRequest{ PutRequest = putRequest });
                batchCounter += 1;
                if (batchCounter % 25 == 0 || batchCounter == SongsToAdd.Count()) {
                    batchDynamodbList.Add(new List<WriteRequest>(dynamodbValues));
                    LambdaLogger.Log($"***INFO: added to batchDynamodbList: {JsonConvert.SerializeObject(batchDynamodbList)}");
                    dynamodbValues = new List<WriteRequest>();
                }
            }
            foreach (var dynamodbList in batchDynamodbList) {
                LambdaLogger.Log($"***INFO: writing to add to database (dynamodbList): {JsonConvert.SerializeObject(dynamodbList)}");
                await DynamodbProvider.DynamoDbBatchWriteItemAsync(dynamodbList);
            }
        }
        
        public void UpdateSummary() {
            LambdaLogger.Log($"Added {SongsToAdd.Count()} Songs");    
            LambdaLogger.Log($"Deleted {SongsToDelete.Count()} Songs");    
        }
    }
}