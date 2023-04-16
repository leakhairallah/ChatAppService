using System.Net;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ChatApp.Web.Storage.Conversations;

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
        try
        {
            var conversation = ToConversation(conversationId, time);

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
        try
        {
            var conversation = ToConversation(conversationId, time);

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
    

    private ConversationEntity ToConversation(string conversationId, long time)
    {
        var conversation = new ConversationEntity(
            id: conversationId,
            partitionKey: conversationId,
            timestamp: time
        );
        return conversation;
    }
    
    public async Task<ConversationEntity> GetConversation(string id)
    {
        QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        using FeedIterator<ConversationEntity> feedIterator = CosmosContainer.GetItemQueryIterator<ConversationEntity>(queryDefinition);

        if (feedIterator.HasMoreResults)
        {
            FeedResponse<ConversationEntity> response = await feedIterator.ReadNextAsync();
            foreach (ConversationEntity item in response)
            {
                return item;
            }
        }

        return null;
        
    }

}
    
