using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.Model;
using Document = System.Reflection.Metadata.Document;

namespace JukeboxAlexa.SonglistIndex {
    public interface IDynamodbDependencyProvider {
        Task<GetItemResponse> DynamodbGetItemAsync(IDictionary<string, AttributeValue> key);
        Task<UpdateItemResponse> DynamodbUpdateItemAsync(Dictionary<string, AttributeValue> key, string updateExpression, IDictionary<string, AttributeValue> expressionAttributeValues);
        Task<PutItemResponse> DynamodbPutItemAsync(IDictionary<string, AttributeValue> key);
        Task DynamodbDeleteWordInDbAsync(IDictionary<string, AttributeValue> key);
    }
}