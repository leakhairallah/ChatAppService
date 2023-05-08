using System.Net;
using Azure;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;

namespace ChatApp.Web.Storage.Images;

public class ImageStore : IImageStore
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<ImageStore> _logger;
    public ImageStore(BlobContainerClient blobContainerClient, ILogger<ImageStore> logger)
    {
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }

    public async Task<UploadImageResponse> UpsertProfilePicture(UploadImageRequest profilePicture)
    {
        _logger.LogInformation("Checking for exceptions...");
        if (profilePicture == null || profilePicture.File.Length == 0)
        {
            throw new InvalidPictureException($"Invalid profile picture {profilePicture}", HttpStatusCode.BadRequest);
        }

        try
        {
            _logger.LogInformation("Uploading image...");
            using var stream = new MemoryStream();
            await profilePicture.File.CopyToAsync(stream);
            stream.Position = 0;

            string imageId = Guid.NewGuid().ToString();
            
            await _blobContainerClient.UploadBlobAsync(imageId, stream);

            return new UploadImageResponse(imageId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public async Task<byte[]> GetProfilePicture(string id)
    {
        _logger.LogInformation("Getting profile picture...");
        try
        {
            var response = await _blobContainerClient.GetBlobClient(id).DownloadAsync();

            await using var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            return bytes;
        }
        catch (RequestFailedException e)
        {
            if (e.Status == 404)
            {
                throw new NotFoundException($"with id {id}", "Image", HttpStatusCode.NotFound);
            }

            throw;
        }
        catch (Exception)
        {
            throw new Exception($"Failed to get profile picture.");
        }
    }

    public async Task DeleteProfilePicture(string id)
    {
        try
        {
            var blobClient = _blobContainerClient.GetBlobClient(id);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (RequestFailedException e)
        {
            if (e.Status == 404)
            {
                throw new Exception("Invalid id.");
            }

            throw;
        }
    }
    
}