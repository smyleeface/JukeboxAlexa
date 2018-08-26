using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Tests {
    public class LocalstackDynamoDb : IDisposable {
        
        public SongFixtures songFixtures = new SongFixtures();
        
        public LocalstackDynamoDb() {
            LocalstackCreateTableAsync(songFixtures.dynamodbClient, songFixtures.tableName, songFixtures.indexNameSearchTitle, songFixtures.indexNameSearchTitleArtist);
            WaitMethod(3000);
            LocalstackPutItemRequest(songFixtures.dynamodbClient, songFixtures.tableName, songFixtures.songAttribute1);
            WaitMethod(3000);
            LocalstackPutItemRequest(songFixtures.dynamodbClient, songFixtures.tableName, songFixtures.songAttribute2);
            WaitMethod(3000);
            LocalstackPutItemRequest(songFixtures.dynamodbClient, songFixtures.tableName, songFixtures.songAttribute3);
            WaitMethod(3000);
            LocalstackPutItemRequest(songFixtures.dynamodbClient, songFixtures.tableName, songFixtures.songAttribute4);
            WaitMethod(3000);
        }

        public void Dispose() {
            LocalstackDeleteTableAsync(songFixtures.dynamodbClient, songFixtures.tableName);
        }

        public async void WaitMethod(int seconds) {
            await Task.Delay(seconds);
        }
        
        public async void LocalstackCreateTableAsync(AmazonDynamoDBClient dynamodbClient, string tableName, string indexNameSearchTitle, string indexNameSearchTitleArtist) {
            var createTableReqeust = new CreateTableRequest {
                AttributeDefinitions= new List<AttributeDefinition> {
                        new AttributeDefinition {
                            AttributeName="track_number",
                            AttributeType="S"
                        },
                        new AttributeDefinition {
                            AttributeName="search_artist",
                            AttributeType="S"
                        },
                        new AttributeDefinition {
                            AttributeName="search_title",
                            AttributeType="S"
                        }
                },
                TableName=tableName,
                KeySchema= new List<KeySchemaElement> {
                    new KeySchemaElement {
                        AttributeName="track_number",
                        KeyType="HASH"
                    }
                },
                GlobalSecondaryIndexes= new List<GlobalSecondaryIndex> {
                    new GlobalSecondaryIndex {
                        IndexName=indexNameSearchTitle,
                        KeySchema=new List<KeySchemaElement> {
                            new KeySchemaElement {
                                AttributeName="search_title",
                                KeyType="HASH"
                            }
                        },
                        Projection= new Projection {
                            ProjectionType="ALL"
                        }, 
                        ProvisionedThroughput= new ProvisionedThroughput {
                            ReadCapacityUnits=1,
                            WriteCapacityUnits=1
                        }
                    },
                    new GlobalSecondaryIndex {
                        IndexName=indexNameSearchTitleArtist,
                        KeySchema=new List<KeySchemaElement> {
                            new KeySchemaElement {
                                AttributeName="search_title",
                                KeyType="HASH"
                            },
                            new KeySchemaElement {
                                AttributeName="search_artist",
                                KeyType="RANGE"
                            }
                        },
                        Projection= new Projection{
                            ProjectionType="ALL"
                        },
                        ProvisionedThroughput= new ProvisionedThroughput {
                            ReadCapacityUnits=1,
                            WriteCapacityUnits=1
                        }
                    }
                },
                ProvisionedThroughput= new ProvisionedThroughput {
                    ReadCapacityUnits=1,
                    WriteCapacityUnits=1
                }
            };
            try {
                await songFixtures.dynamodbClient.CreateTableAsync(createTableReqeust);
                Console.WriteLine($"*** INFO: table `{tableName}` created");
            }
            catch (Exception e) {
                Console.WriteLine($"*** ERROR: problem creating table `{tableName}`: {e}");
            }
        }

        public async void LocalstackPutItemRequest(AmazonDynamoDBClient dynamoDbClient, string tableName, Dictionary<string, AttributeValue> song) {
            var putItemRequest = new PutItemRequest {
                TableName=tableName,
                Item=song
            };
            await dynamoDbClient.PutItemAsync(putItemRequest);
        }
        
        public async void LocalstackDescribeTable(AmazonDynamoDBClient dynamoDbClient, string tableName) {
            var describeTableRequest = new DescribeTableRequest {
                TableName=tableName
            };
            await dynamoDbClient.DescribeTableAsync(describeTableRequest);
        }

        public async void LocalstackDeleteTableAsync(AmazonDynamoDBClient dynamoDbClient, string tableName) {
            await songFixtures.dynamodbClient.DeleteTableAsync(new DeleteTableRequest {
                TableName = tableName
            });
        }        
    }
}