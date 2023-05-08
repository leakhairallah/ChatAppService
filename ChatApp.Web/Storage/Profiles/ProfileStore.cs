using System.Net;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos.Linq;

namespace ChatApp.Web.Storage.Profiles;

public class ProfileStore : IProfileStore
{
    private readonly CosmosClient _cosmosClient;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<ProfileStore> _logger;
    public ProfileStore(CosmosClient cosmosClient, BlobContainerClient blobContainerClient, ILogger<ProfileStore> logger)
    {
        _cosmosClient = cosmosClient;
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }

    // DRY
    //private Container CosmosContainer => _cosmosClient.GetDatabase("chatapi").GetContainer("profiles");
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("profiles");

    public async Task UpsertProfile(Profile profile)
    {
        _logger.LogInformation("Checking for exceptions...");
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.Username) ||
            string.IsNullOrWhiteSpace(profile.FirstName) ||
            string.IsNullOrWhiteSpace(profile.LastName) ||
            string.IsNullOrWhiteSpace(profile.ProfilePictureId)
           )
        {
            throw new InvalidProfileException($"Invalid profile {profile}", HttpStatusCode.BadRequest);
        }

        try
        {
            _logger.LogInformation("Updating profile for {username}", profile.Username);
            await CosmosContainer.UpsertItemAsync(ToEntity(profile));
        }
        catch (CosmosException)
        {
            throw new Exception($"Failed to create profile for user {profile.Username}");
        }
    }

    public async Task AddProfile(Profile profile)
    {
        _logger.LogInformation("Checking for exceptions...");
        try
        {
            ValidateProfile(profile);
                
            _logger.LogInformation("Creating profile for user {username}...", profile.Username);
            await CosmosContainer.CreateItemAsync(ToEntity(profile));
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConflictException($"with username {profile.Username}", "Profile", HttpStatusCode.NotFound);
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
            throw new InvalidProfileException($"Invalid profile {profile}", HttpStatusCode.BadRequest);
        }
    }

    public async Task<Profile?> GetProfile(string username)
    {
        _logger.LogInformation("Getting profile of {username}", username);
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
                throw new NotFoundException($"with username {username}", "User", HttpStatusCode.NotFound);
            }
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