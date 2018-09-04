using System.Collections.Generic;
using System.Threading.Tasks;
using JukeboxAlexa.Library.Model;

namespace JukeboxAlexa.PlaySongTitleRequest {
    public interface IDynamodbDependencyProvider {
        Task<IEnumerable<SongModel.Song>> DynamoDbFindSongsByTitleAsync(string title);
    }
}