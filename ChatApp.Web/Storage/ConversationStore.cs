using System.Net;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ChatApp.Web.Storage;

public class ConversationStore: IConversationStore
{
    private readonly CosmosClient _cosmosClient;
    
    public ConversationStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }
    

    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversations");

    public async Task<HttpStatusCode> AddConversation(string conversationId, long time)
    {
        //TODO: implement these checks in the service layer
        if (string.IsNullOrWhiteSpace(conversationId) || time.IsNull())
        {
            throw new ArgumentException($"Invalid input {conversationId}", nameof(conversationId));
        }

        try
        {
            //TODO: create toConversation function 
            var conversation = new ConversationEntity(
                id: conversationId,
                partitionKey: conversationId,
                timestamp: time
            );

            await CosmosContainer.CreateItemAsync(conversation);

            return HttpStatusCode.OK;
        }

        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.Conflict)
            {
                return e.StatusCode;
            }

            throw;
        }
    }


    public async Task<HttpStatusCode> UpdateConversation(string conversationId, long time)
    {
        //TODO: implement these checks in the service layer
        if (string.IsNullOrWhiteSpace(conversationId) || time.IsNull() || time > DateTime.UtcNow.Millisecond)
        {
            throw new ArgumentException($"Invalid input {conversationId}", nameof(conversationId));
        }

        try
        {
            //TODO: create toConversation function 
            var conversation = new ConversationEntity(
                id: conversationId,
                partitionKey: conversationId,
                timestamp: time
            );

            await CosmosContainer.UpsertItemAsync(conversation);

            return HttpStatusCode.OK;
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return e.StatusCode;
            }

            throw;
        }
    }
}
    
