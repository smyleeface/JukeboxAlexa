using System.Collections.Generic;
using System.Threading.Tasks;
using JukeboxAlexa.Library.Model;

namespace JukeboxAlexa.PlaySongTitleArtistRequest {
    public interface IDynamodbDependencyProvider {
        Task<IEnumerable<SongModel.Song>> DynamoDbFindSongsByTitleArtistAsync(string title, string artist);
    }
}