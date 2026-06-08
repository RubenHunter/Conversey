using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Conversey.DAL.Administration;

public class CloudStorageRepository : ICloudStorageRepository
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

     public CloudStorageRepository(IConfiguration config)
     {
         _bucketName = config["GoogleCloud:StorageBucket"] 
                       ?? config["GoogleCloud:BucketName"]
                       ?? throw new InvalidOperationException("GoogleCloud Storage bucket name is missing. Check 'GoogleCloud:StorageBucket' context.");

         // This single line replaces all the FileStream and ServiceAccount logic.
         // It will automatically find the credentials from the gcloud CLI locally
         // or the environment in production.
         _storageClient = StorageClient.Create();
     }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var objectName = $"project-images/{Guid.NewGuid()}-{NormalizeFileName(fileName)}";
        
        // Makes sure that the stream is at the beginning
        if (fileStream.CanSeek) fileStream.Position = 0;

        var data = await _storageClient.UploadObjectAsync(
            _bucketName, 
            objectName, 
            contentType, 
            fileStream
        );
        
        return $"https://storage.googleapis.com/{_bucketName}/{data.Name}";
    }

    private static string NormalizeFileName(string fileName)
    {
        var rawName = Path.GetFileName(fileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return "upload";
        }

        var normalized = Regex.Replace(rawName.Trim(), @"\s+", "-");
        normalized = Regex.Replace(normalized, @"[^A-Za-z0-9._-]", string.Empty);
        return normalized.Length == 0 ? "upload" : normalized;
    }

}
