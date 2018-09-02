using System.Threading.Tasks;

namespace JukeboxAlexa.SonglistUpload {
    public interface IS3DependencyProvider {
        Task<string> GetSongsFromS3UploadAsync(string bucket, string key);
    }
}