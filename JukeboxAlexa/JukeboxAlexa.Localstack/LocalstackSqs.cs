using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using JukeboxAlexa.Library.Model;
using Newtonsoft.Json;

namespace JukeboxAlexa.Library.Tests {
    public class LocalstackSqs : IDisposable {
        
        public QueueFixtures queueFixtures = new QueueFixtures();
        
        public LocalstackSqs() {
            Console.WriteLine("*** INFO: Starting localstackSqs");
            LocalstackCreateQueueAsync(queueFixtures.sqsClient, queueFixtures.jukeboxQueueName);
        }

        public void Dispose() {
            LocalstackDeleteQueueAsync(queueFixtures.sqsClient, queueFixtures.jukeboxQueueName);
        }

        public async void WaitMethod(int seconds) {
            await Task.Delay(seconds);
        }
        
        public async void LocalstackCreateQueueAsync(AmazonSQSClient amazonSQSClient, string queueName) {
            var createQueueRequest = new CreateQueueRequest {
                QueueName = queueName
            };
            Console.WriteLine($"*** INFO: creating queue `{queueName}`");
            try {
                await amazonSQSClient.CreateQueueAsync(createQueueRequest);
                Console.WriteLine($"*** INFO: queue `{queueName}` created");
            }
            catch (Exception e) {
                Console.WriteLine($"*** ERROR: problem creating queue `{queueName}`: {e}");
            }
        }          
        
        public async void LocalstackDeleteQueueAsync(AmazonSQSClient amazonSQSClient, string queueName) {
            var createQueueRequest = new GetQueueUrlRequest {
                QueueName = queueName
            };
            try {
                var queueUrlResponse = await amazonSQSClient.GetQueueUrlAsync(createQueueRequest);
                await amazonSQSClient.DeleteQueueAsync(
                    new DeleteQueueRequest {
                        QueueUrl = queueUrlResponse.QueueUrl
                    }
                );
                Console.WriteLine($"*** INFO: queue `{queueName}` deleted");
            }
            catch (Exception e) {
                Console.WriteLine($"*** ERROR: problem deleting queue `{queueName}`: {e}");
            }
        }        
    }
}