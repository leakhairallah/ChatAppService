using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Storage.Messages;

//TODO: check for any missing exceptions

public class MessageStore : IMessageStore
{
    private readonly CosmosClient _cosmosClient;

    public MessageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }
    
    //private Container MessageContainer => _cosmosClient.GetDatabase("chatapi").GetContainer("messages");
    private Container MessageContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("messages");
    //private Container ConversationsContainer => _cosmosClient.GetDatabase("chatapi").GetContainer("conversations");
    private Container ConversationsContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversations");
    //private Container ProfilesContainer => _cosmosClient.GetDatabase("chatapi").GetContainer("profiles");
    private Container ProfilesContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("profiles");
    
    public async Task<UploadMessageResponse?> PostMessageToConversation(string conversationId, SendMessageRequest msg, long datetime)
    {
        if (msg == null ||
            string.IsNullOrWhiteSpace(msg.Id) ||
            string.IsNullOrWhiteSpace(msg.Text) ||
            string.IsNullOrWhiteSpace(msg.SenderUsername)
           )
        {
            throw new ArgumentException($"Invalid message {msg}", nameof(msg));
        }
        
        try
        {
            await ConversationsContainer.ReadItemAsync<ConversationEntity>(
                id: conversationId,
                partitionKey: new PartitionKey(conversationId),
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

        var messageEntity = ToEntity(conversationId, msg, datetime);

        await MessageContainer.CreateItemAsync(messageEntity);
        return new UploadMessageResponse(messageEntity.Timestamp);
    }

    public async Task<GetConversationResponse?> GetMessageFromConversation(string conversationId, PaginationFilter filter)
    {
        try
        {
            var parameterizedQuery =
                new QueryDefinition(
                        "SELECT * FROM c WHERE c.partitionKey = @conversationId AND c.Timestamp > @lastSeenMessage ORDER BY c.Timestamp DESC")
                    .WithParameter("@conversationId", conversationId)
                    .WithParameter("@lastSeenMessage", filter.lastSeenMessageTime);
            QueryRequestOptions options = new QueryRequestOptions() { MaxItemCount = filter.limit };
            
            Console.WriteLine("store :" + filter.lastSeenMessageTime);
            using FeedIterator<MessageEntity> filteredFeed = MessageContainer.GetItemQueryIterator<MessageEntity>(
                queryDefinition: parameterizedQuery,
                requestOptions: options,
                continuationToken: string.IsNullOrEmpty(filter.ContinuationToken) ? null : filter.ContinuationToken);
            
            List<GetMessageResponse> messageResponses = new List<GetMessageResponse>();
            string newContinuationToken = "";

            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<MessageEntity> messageEntities = await filteredFeed.ReadNextAsync();
                messageResponses.AddRange(ToMessages(messageEntities));
                newContinuationToken = messageEntities.ContinuationToken;
                if (messageResponses.Count >= filter.limit)
                {
                    break;
                }
            }

            return new GetConversationResponse(messageResponses, newContinuationToken);
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

    public async Task<MessageConflict> MessageConflictCheck(string conversationId, SendMessageRequest msg)
    {
        try
        {
            await MessageContainer.ReadItemAsync<MessageEntity>(
                id: msg.Id,
                partitionKey: new PartitionKey(msg.Id),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return new MessageConflict(true);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return  new MessageConflict(false);
            }

            throw;
        }
    }
    
    private static MessageEntity ToEntity(string conversationId, SendMessageRequest msg, long datetime)
    {
        return new MessageEntity(
            partitionKey: conversationId, 
            id: msg.Id,
            SenderUsername: msg.SenderUsername,
            Content: msg.Text,
            Timestamp: datetime
        );
    }

    private static IEnumerable<GetMessageResponse> ToMessages(IEnumerable<MessageEntity> messages)
    {
        return messages.Select(msg => new GetMessageResponse(
            msg.Content,
            msg.SenderUsername,
            msg.Timestamp
        ));
    }
}