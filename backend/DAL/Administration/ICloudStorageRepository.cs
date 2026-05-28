namespace Conversey.DAL.Administration;

public interface ICloudStorageRepository
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
}