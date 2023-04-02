using System.Net;
using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
namespace ChatApp.Web.Storage;

public class ConversationParticipantsStore : IConversationParticipantsStore
{
    private readonly CosmosClient _cosmosClient;
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversationParticipants");
    
    public ConversationParticipantsStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }
    
    public async Task<string> AddConversation(string participant1, string participant2)
    {
        var id = GenerateId();
        //TODO: check if id already exists is db
        //TODO: check if these users already have a conversation in the db in the SERVICE LAYER
        var conversation1 = new ConversationParticipants(
            PartitionKey: id,
            Participant: participant1
        );
        
        var conversation2 = new ConversationParticipants(
            PartitionKey: id,
            Participant: participant2
        );
        
        try
        { 
            await CosmosContainer.CreateItemAsync(conversation1);
        }

        catch (CosmosException e)
        {
            throw new Exception($"Failed to create conversation for {participant1} with ConversationId {id}");

        }
        try
        { 
            await CosmosContainer.CreateItemAsync(conversation2);
        }

        catch (CosmosException e)
        {
            throw new Exception($"Failed to create conversation for {participant2} with ConversationId {id}");

        }
        return id;
    }
    
    //TODO: 
    // public Task<HttpStatusCode> DeleteConversation(string conversationId)
    // {
    //     
    // }
    
    string GenerateId()
    {
        return Guid.NewGuid().ToString("N");
    }
}