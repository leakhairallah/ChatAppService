using System.Net;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ChatApp.Web.Storage.Messages;

//TODO: check for any missing exceptions

public class MessageStore : IMessageStore
{
    private readonly CosmosClient _cosmosClient;

    public MessageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }
    
    private Container MessageContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("messages");
    private Container ConversationsContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversations");
    private Container ProfilesContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("profiles");
    
    public async Task<UploadMessageResponse?> PostMessageToConversation(PostMessage msg)
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
        await MessageContainer.CreateItemAsync(messageEntity);
        
        return new UploadMessageResponse(messageEntity.Timestamp);
    }

    public async Task<GetConversationResponse?> GetMessageFromConversation(string conversationId, PaginationFilter filter)
    {
        try
        {
            var parameterizedQuery =
                new QueryDefinition(
                        "SELECT * FROM c WHERE c.partitionKey = @conversationId AND c.Timestamp > @lastSeenMessage")
                    .WithParameter("@conversationId", conversationId)
                    .WithParameter("@lastSeenMessage", filter.LastSeenMessageTime);
            QueryRequestOptions options = new QueryRequestOptions() { MaxItemCount = filter.PageSize };

            using FeedIterator<MessageEntity> filteredFeed = MessageContainer.GetItemQueryIterator<MessageEntity>(
                queryDefinition: parameterizedQuery,
                requestOptions: options);
                ///continuationToken: string.IsNullOrEmpty(filter.ContinuationToken) ? null : filter.ContinuationToken);
            
            string newContinuationToken = "";
            var messages = new List<GetMessageResponse>();
            
            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<MessageEntity> response = await filteredFeed.ReadNextAsync();

                foreach (MessageEntity messageEntity in response)
                {
                    messages.Add(ToMessage(messageEntity));
                }
                
                if (response.Count > filter.PageSize - 1)
                {
                    newContinuationToken = response.ContinuationToken;
                }
            }

            return new GetConversationResponse(messages, newContinuationToken);
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
    
    private static MessageEntity ToEntity(PostMessage msg)
    {
        return new MessageEntity(
            partitionKey: msg.ConversationId, 
            id: Guid.NewGuid().ToString(),
            SenderUsername: msg.SenderUsername,
            Content: msg.Content,
            Timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );
    }

    private static GetMessageResponse ToMessage(MessageEntity msg)
    {
        return new GetMessageResponse(
            msg.Content,
            msg.SenderUsername,
            msg.Timestamp
        );
    }
}