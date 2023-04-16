using System.Net;
using Azure;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Entities;

namespace ChatApp.Web.Storage.Images;

public class ImageStore : IImageStore
{
    
    private readonly CosmosClient _cosmosClient;
    private readonly BlobContainerClient _blobContainerClient;
    public ImageStore(CosmosClient cosmosClient, BlobContainerClient blobContainerClient)
    {
        _cosmosClient = cosmosClient;
        _blobContainerClient = blobContainerClient;
    }
    
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("profiles");

    public async Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest profilePicture)
    {
        if (profilePicture == null || profilePicture.File.Length == 0)
        {
            throw new ArgumentException($"Invalid profile picture {profilePicture}", nameof(profilePicture));
        }

        try
        {
            using var stream = new MemoryStream();
            await profilePicture.File.CopyToAsync(stream);
            stream.Position = 0;

            string imageId = Guid.NewGuid().ToString();


            await _blobContainerClient.UploadBlobAsync(imageId, stream);
            return new UploadImageResponse(imageId);
        }
        catch (Exception e)
        {
            throw new Exception( e.Message);
        }
    }
    public async Task<byte[]?> GetProfilePicture(string username)
    {
        try
        {
            var entity = await CosmosContainer.ReadItemAsync<ProfileEntity>(
                id: username,
                partitionKey: new PartitionKey(username),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            
            var response = await _blobContainerClient.GetBlobClient(ToProfile(entity).ProfilePictureId).DownloadAsync();

            await using var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            return bytes;
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException("Username not found");
            }

            throw;
        }
        catch (RequestFailedException e)
        {
            if (e.Status == 404)
            {
                throw new Exception("Could not download image.");
            }
            throw;
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
                throw new Exception("Invalid id");
            }

            throw;
        }
    }
    
    private static Profile ToProfile(ProfileEntity entity)
    {
        return new Profile(
            Username: entity.id,
            entity.FirstName,
            entity.LastName,
            entity.ProfilePictureId
        );
    }


}