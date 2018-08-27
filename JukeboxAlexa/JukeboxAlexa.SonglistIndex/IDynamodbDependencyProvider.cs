using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace JukeboxAlexa.SonglistIndex {
    public interface IDynamodbDependencyProvider {
        Task<GetItemResponse> DynamodbGetItemAsync(IDictionary<string, AttributeValue> key);
        Task<UpdateItemResponse> DynamodbUpdateItemAsync(Dictionary<string, AttributeValue> key, string updateExpression, IDictionary<string, AttributeValue> expressionAttributeValues);
    }
}