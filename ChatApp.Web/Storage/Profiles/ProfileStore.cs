using System.Net;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos.Linq;

namespace ChatApp.Web.Storage.Profiles;

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
    //private Container CosmosContainer => _cosmosClient.GetDatabase("chatapi").GetContainer("profiles");
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("profiles");

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

    public async Task AddProfile(Profile profile)
    {
        try
        {
            ValidateProfile(profile);
            await CosmosContainer.CreateItemAsync(ToEntity(profile));
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.Conflict)
            {
                throw new Exception($"A user with username {profile.Username} already exists");
            }
        }
    }
    
    private static void ValidateProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.Username) ||
            string.IsNullOrWhiteSpace(profile.FirstName) ||
            string.IsNullOrWhiteSpace(profile.LastName)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }
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


    public async Task DeleteProfile(string username)
    {
        try
        {
            await CosmosContainer.DeleteItemAsync<Profile>(
                id: username,
                partitionKey: new PartitionKey(username)
            );
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            throw;
        }
    }
    
    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            partitionKey: profile.Username,
            id: profile.Username,
            FirstName: profile.FirstName,
            LastName: profile.LastName,
            ProfilePictureId: profile.ProfilePictureId
        );
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