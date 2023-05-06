using System.Net;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Storage.Conversations;

public class ConversationStore: IConversationStore
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<ConversationStore> _logger;
    
    public ConversationStore(CosmosClient cosmosClient, ILogger<ConversationStore> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
    }
    

    //private Container CosmosContainer => _cosmosClient.GetDatabase("chatapi").GetContainer("conversations");
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversations");

    public async Task<string?> AddConversation(string conversationId, long time)
    {
        using (_logger.BeginScope("Creating conversation with conversation Id {conversationId}...", conversationId))
        {
            try
            {
                var conversation = ToConversation(conversationId, time);

                await CosmosContainer.CreateItemAsync(conversation);

                return conversationId;
            }

            catch (CosmosException e)
            {
                return null;
            }
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
    
    public async Task<ConversationEntity?> GetConversation(string id)
    {
        using (_logger.BeginScope("Getting conversations with user Id {userId}...", id))
        {
            try
            {
                QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);

                using FeedIterator<ConversationEntity> feedIterator =
                    CosmosContainer.GetItemQueryIterator<ConversationEntity>(queryDefinition);

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
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
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
    
    

}
    
