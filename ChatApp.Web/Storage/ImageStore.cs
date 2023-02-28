using System.Net;
using Azure;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Entities;
using Microsoft.AspNetCore.Mvc;


namespace ChatApp.Web.Storage;

public class ImageStore : IImageStore
{
    
    private readonly CosmosClient _cosmosClient;
    private readonly BlobContainerClient _blobContainerClient;
    public ImageStore(CosmosClient cosmosClient, BlobContainerClient blobContainerClient)
    {
        _cosmosClient = cosmosClient;
        _blobContainerClient = blobContainerClient;
    }
    
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("sharedContainer");

    public async Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest profilePicture)
    {
        if (profilePicture == null || 
            profilePicture.File.Length == 0)
        {
            throw new ArgumentException($"Invalid profile picture {profilePicture}", nameof(profilePicture));
        }
        
        using var stream = new MemoryStream();
        await profilePicture.File.CopyToAsync(stream);
        stream.Position = 0;
        
        string imageId = Guid.NewGuid().ToString();
        Console.WriteLine(imageId);

        await _blobContainerClient.UploadBlobAsync(imageId, stream);
        return new UploadImageResponse(imageId);
        
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
        catch (RequestFailedException e)
        {
            if (e.Status == 404)
            {
                return null;
            }
            throw;
        }
    }

    public async Task DeleteProfilePicture(string id)
    {
        BlobClient blobClient = _blobContainerClient.GetBlobClient(id);
        
        try
        {
            await blobClient.DeleteIfExistsAsync();
        }
        catch (RequestFailedException e)
        {
            if (e.Status == 404)
            {
                return;
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