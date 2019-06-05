using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace JukeboxAlexa.SonglistUpload {
    public interface IS3DependencyProvider {
        Task<GetObjectResponse> S3GetObjectAsync(string bucket, string key);
        string ReadS3Stream(Stream stream);
    }
}