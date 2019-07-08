using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using JukeboxAlexa.Library.Model;

namespace JukeboxAlexa.PlaySongTitleRequest {
    public interface IDynamodbDependencyProvider {
        Task<IEnumerable<SongModel.Song>> DynamoDbFindSongsByTitleAsync(string title);
        Task<GetItemResponse> DynamoDbFindSimilarSongsAsync(IDictionary<string, AttributeValue> key);
    }
}