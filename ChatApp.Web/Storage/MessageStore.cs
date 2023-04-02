using System.Net;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Storage;

//TODO: check for any missing exceptions

public class MessageStore : IMessageStore
{
    private readonly CosmosClient _cosmosClient;

    public MessageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    // DRY
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("messages");
    private Container ConversationsContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversations");
    private Container ProfilesContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("profiles");
    
    public async Task<UploadMessageResponse?> PostMessageToConversation(Message msg)
    {
        if (msg == null ||
            string.IsNullOrWhiteSpace(msg.ConversationId) ||
            string.IsNullOrWhiteSpace(msg.Content) ||
            string.IsNullOrWhiteSpace(msg.SenderUsername)
           )
        {
            throw new ArgumentException($"Invalid message {msg}", nameof(msg));
        }
        
        try
        {
            await ConversationsContainer.ReadItemAsync<ConversationEntity>(
                id: msg.ConversationId,
                partitionKey: new PartitionKey(msg.ConversationId),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
        }

        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException($"Conversation doesn't exist, please create one first.");

            }
        }
        
        try
        {
            
            await ProfilesContainer.ReadItemAsync<ProfileEntity>(
                id: msg.SenderUsername,
                partitionKey: new PartitionKey(msg.SenderUsername),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
        }

        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException($"User does not exist.");
            }
        }

        var messageEntity = ToEntity(msg);
        await CosmosContainer.CreateItemAsync(messageEntity);
        
        return new UploadMessageResponse(messageEntity.Timestamp);
    }

    public async Task<UserConversation?> GetMessageFromConversation(string conversationId)
    {
        try
        {
            var parameterizedQuery = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @conversationId")
                .WithParameter("@conversationId", conversationId);

            var messages = new List<Message>();
            
            using FeedIterator<MessageEntity> filteredFeed = CosmosContainer.GetItemQueryIterator<MessageEntity>(
                queryDefinition: parameterizedQuery
            );
            
            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<MessageEntity> response = await filteredFeed.ReadNextAsync();
                // Iterate query results
                foreach (MessageEntity messageEntity in response)
                {
                    messages.Add(ToMessage(messageEntity));
                }
            }

            return new UserConversation(messages);
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
    
    private static MessageEntity ToEntity(Message msg)
    {
        return new MessageEntity(
            partitionKey: msg.ConversationId, 
            id: Guid.NewGuid().ToString(),
            SenderUsername: msg.SenderUsername,
            Content: msg.Content,
            Timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );
    }

    private static Message ToMessage(MessageEntity msg)
    {
        return new Message(
            msg.partitionKey,
            msg.Content,
            msg.SenderUsername
        );
    }
}