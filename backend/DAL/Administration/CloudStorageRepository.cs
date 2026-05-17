using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace Conversey.DAL.Administration;

public class CloudStorageRepository : ICloudStorageRepository
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    /*public CloudStorageRepository(IConfiguration config)
    {
        _bucketName = config["GoogleCloud:BucketName"] 
                      ?? throw new InvalidOperationException("GoogleCloud:BucketName is missing from configuration.");

        var keyPath = config["GoogleCloud:JsonFilePath"] 
                      ?? throw new InvalidOperationException("GoogleCloud:JsonFilePath is missing from configuration.");

        using var stream = new FileStream(keyPath, FileMode.Open, FileAccess.Read);
    
        var serviceAccount = CredentialFactory.FromStream<ServiceAccountCredential>(stream);
        var credential = serviceAccount.ToGoogleCredential();

        _storageClient = StorageClient.Create(credential);
    }*/

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var objectName = $"project-images/{Guid.NewGuid()}-{fileName}";
        
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
}