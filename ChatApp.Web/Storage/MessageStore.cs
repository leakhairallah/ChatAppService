using System.Net;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Storage;

public class MessageStore : IMessageStore
{
    private readonly CosmosClient _cosmosClient;

    public MessageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    // DRY
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("messages");
    
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
            var messageEntity = ToEntity(msg);
            await CosmosContainer.CreateItemAsync(messageEntity);
            
            return new UploadMessageResponse(messageEntity.Timestamp);
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

    public async Task<UserConversation?> GetMessageFromConversation()
    {
        return null;
    }
    
    private static MessageEntity ToEntity(Message msg)
    {
        return new MessageEntity(
            partitionKey: msg.ConversationId, 
            MessageId: Guid.NewGuid().ToString(),
            SenderUsername: msg.SenderUsername,
            Content: msg.Content,
            Timestamp: DateTime.Now
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