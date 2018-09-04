using System.Collections.Generic;
using System.Threading.Tasks;
using JukeboxAlexa.Library.Model;

namespace JukeboxAlexa.PlaySongNumberRequest {
    public interface IDynamodbDependencyProvider {
        Task<IEnumerable<SongModel.Song>> DynamoDbFindSongsByNumberAsync(string title);
    }
}