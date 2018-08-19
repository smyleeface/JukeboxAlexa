using System.Threading.Tasks;
using Amazon.S3.Model;

namespace JukeboxAlexa.SonglistUpload {
    public interface IS3DependencyProvider {
        Task<string> GetSongsFromS3UploadAsync(string bucket, string key);
    }
}