using ChatApp.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
namespace ChatApp.Web.Storage.Conversations;

public class ConversationParticipantsStore : IConversationParticipantsStore
{
    private readonly CosmosClient _cosmosClient;
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversationParticipants");
    private readonly IConversationStore _conversationStore;
    
    public ConversationParticipantsStore(CosmosClient cosmosClient, IConversationStore conversationStore)
    {
        _cosmosClient = cosmosClient;
        _conversationStore = conversationStore;
    }

    
    public async Task<string> AddConversation(string participant1, string participant2)
    {
        var id = participant1 + "_" + participant2;

        var response1 = await _conversationStore.GetConversation(id);
        if (response1 != null)
        {
            throw new Exception($"Conversation already exists");
        }

        var tempId = participant2 + "_" + participant1;
        
        var response2 = await _conversationStore.GetConversation(tempId);
        if (response2 != null)
        {
            throw new Exception($"Conversation already exists");
        }
        
        try
        { 
            await CosmosContainer.CreateItemAsync(ToConversation(id, participant1));
        }

        catch (CosmosException e)
        {
            throw new Exception($"Failed to create conversation for {participant1} with ConversationId {id}");
        }
        try
        { 
            await CosmosContainer.CreateItemAsync(ToConversation(tempId, participant2));   
        }

        catch (CosmosException e)
        {
            throw new Exception($"Failed to create conversation for {participant2} with ConversationId {tempId}");
        }
        return id;
    }
    
    private ConversationParticipants ToConversation(string id, string participant)
    {
        return new ConversationParticipants(
            id: id+"_"+participant,
            partitionKey: id,
            Participant: participant
        );
    }
}
