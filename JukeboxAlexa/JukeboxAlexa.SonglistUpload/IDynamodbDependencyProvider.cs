using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.Model;

namespace JukeboxAlexa.SonglistUpload {
    public interface IDynamodbDependencyProvider {
        Task<IEnumerable<SongModel.Song>> DynamoDbFindSongsByTitleAsync(string title);
        Task<ScanResponse> DynamoDbScanAsync();
        Task<BatchWriteItemResponse> DynamoDbBatchWriteItemAsync(IEnumerable<WriteRequest> writeRequests);
    }
}