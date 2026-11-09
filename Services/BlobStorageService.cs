using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace RaceDay.Api.Services;
public class BlobStorageService(IConfiguration configuration)
{
    public async Task<string> UploadAsync(IFormFile file,string folder,CancellationToken cancellationToken=default)
    {
        var connection=configuration["AzureBlobStorage:ConnectionString"];
        var containerName=configuration["AzureBlobStorage:ContainerName"]??"raceday-media";
        if(string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage:ConnectionString.");
        if(file.Length==0||file.Length>5*1024*1024) throw new InvalidOperationException("Choose an image smaller than 5 MB.");
        var allowed=new[]{"image/jpeg","image/png","image/webp"};
        if(!allowed.Contains(file.ContentType,StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException("Only JPG, PNG and WebP images are accepted.");
        var service=new BlobServiceClient(connection); var container=service.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob,cancellationToken:cancellationToken);
        var extension=Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
        var blob=container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension}");
        await using var stream=file.OpenReadStream();
        await blob.UploadAsync(stream,new BlobHttpHeaders{ContentType=file.ContentType},cancellationToken:cancellationToken);
        return blob.Uri.ToString();
    }
}
