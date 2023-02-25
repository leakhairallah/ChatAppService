using System.Net;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Entities;

namespace ChatApp.Web.Storage;

public class ProfileStore : IProfileStore
{
    private readonly CosmosClient _cosmosClient;
    private readonly BlobContainerClient _blobContainerClient;
    public ProfileStore(CosmosClient cosmosClient, BlobContainerClient blobContainerClient)
    {
        _cosmosClient = cosmosClient;
        _blobContainerClient = blobContainerClient;
    }

    // DRY
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("sharedContainer");

    public async Task UpsertProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.Username) ||
            string.IsNullOrWhiteSpace(profile.FirstName) ||
            string.IsNullOrWhiteSpace(profile.LastName) ||
            string.IsNullOrWhiteSpace(profile.ProfilePictureId)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }
        
        
        await CosmosContainer.UpsertItemAsync(ToEntity(profile));

    }

    public async Task<Profile?> GetProfile(string username)
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
            return ToProfile(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            throw;
        }
    }
    
    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            PartitionKey: profile.Username,
            Id: profile.Username,
            profile.FirstName,
            profile.LastName,
            profile.ProfilePictureId
        );
    }

    private static Profile ToProfile(ProfileEntity entity)
    {
        return new Profile(
            Username: entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.ProfilePictureId
        );
    }
}