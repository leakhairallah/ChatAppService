using System.Web;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Storage.Entities;
using ChatApp.Web.Storage.Profiles;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Converters;

namespace ChatApp.Web.Storage.Conversations;

public class ConversationParticipantsStore : IConversationParticipantsStore
{
    private readonly CosmosClient _cosmosClient;
    private Container CosmosContainer => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversationParticipants");
    //private Container CosmosContainer => _cosmosClient.GetDatabase("chatapi").GetContainer("conversationParticipants");
    private Container CosmosContainer2 => _cosmosClient.GetDatabase("ChatAppDatabase").GetContainer("conversations");
    //private Container CosmosContainer2 => _cosmosClient.GetDatabase("chatapi").GetContainer("conversations");
    private readonly IProfileStore _profileStore;
    private readonly IConversationStore _conversationStore;
    
    
    public ConversationParticipantsStore(CosmosClient cosmosClient, IConversationStore conversationStore, IProfileStore profileStore)
    {
        _cosmosClient = cosmosClient;
        _conversationStore = conversationStore;
        _profileStore = profileStore;
    }
    
    public async Task<List<ConversationParticipants>?> GetConversation(string id)
    {
        List<ConversationParticipants>? list = null;
        QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        using FeedIterator<ConversationParticipants> feedIterator = CosmosContainer.GetItemQueryIterator<ConversationParticipants>(queryDefinition);

        if (feedIterator.HasMoreResults)
        {
            FeedResponse<ConversationParticipants> response = await feedIterator.ReadNextAsync();
            foreach (ConversationParticipants item in response)
            {
                list?.Add(item);
            }
        }

        return list;
        
    }

    
    public async Task<string> AddConversation(string participant1, string participant2, string id)
    {
        var response1 = await GetConversation(id);
        if (response1 != null && response1.Count != 0)
        {
            if (response1.Count == 1)
            {
                var participant = response1.First().Participant;
                if (participant == participant1)
                {
                    try
                    { 
                        await CosmosContainer.CreateItemAsync(ToConversation(id, participant2));   
                    }

                    catch (CosmosException e)
                    {
                        throw new Exception($"Failed to create conversation for {participant2} with ConversationId {id}");
                    }  
                }
                else if (participant == participant2)
                {
                    try
                    { 
                        await CosmosContainer.CreateItemAsync(ToConversation(id, participant1));
                    }

                    catch (CosmosException e)
                    {
                        throw new Exception($"Failed to create conversation for {participant1} with ConversationId {id}");
                    }
                }
            }
            throw new Exception($"Conversation already exists");
        }

        var tempId = participant2 + "_" + participant1;
        
        var response2 = await GetConversation(tempId);
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
            await CosmosContainer.CreateItemAsync(ToConversation(id, participant2));   
        }

        catch (CosmosException e)
        {
            throw new Exception($"Failed to create conversation for {participant2} with ConversationId {id}");
        }
        return id;
    }
    
    public async Task<GetUserConversations> GetConversations(string participant, PaginationFilter filter)
    {
        List<ConversationResponse> userConversations = new List<ConversationResponse>();
        if (userConversations == null) throw new ArgumentNullException(nameof(userConversations));
        string newContinuationToken = "";
         
        var parameterizedQuery =
            new QueryDefinition("SELECT * FROM c WHERE c.partitionKey LIKE @participant")
                .WithParameter("@participant", '%'+participant+'%');
        QueryRequestOptions options = new QueryRequestOptions() { MaxItemCount = filter.PageSize };
        
        using FeedIterator<ConversationEntity> feedIterator = CosmosContainer2.GetItemQueryIterator<ConversationEntity>(
            queryDefinition: parameterizedQuery,
            requestOptions: options,
            continuationToken: string.IsNullOrEmpty(filter.ContinuationToken) ? null : HttpUtility.UrlDecode(filter.ContinuationToken).Replace("\\", ""));

        while (feedIterator.HasMoreResults)
        {
            FeedResponse<ConversationEntity> response = await feedIterator.ReadNextAsync();
            foreach (ConversationEntity conversation in response)
            {
                Console.WriteLine(conversation.partitionKey);
                QueryDefinition query = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @id")
                    .WithParameter("@id", conversation.partitionKey);

                using FeedIterator<ConversationParticipants> conversationIterator =
                    CosmosContainer.GetItemQueryIterator<ConversationParticipants>(queryDefinition: query,
                        requestOptions: options);

                while (conversationIterator.HasMoreResults)
                {
                    FeedResponse<ConversationParticipants> conversationResponse =
                        await conversationIterator.ReadNextAsync();
                    foreach (ConversationParticipants convParticipants in conversationResponse)
                    {
                        if (convParticipants.Participant != participant)
                        {
                            var profile = _profileStore.GetProfile(convParticipants.Participant);
                            var conv = new ConversationResponse(conversation.partitionKey, conversation.timestamp,
                                profile.Result);
                            userConversations.Add(conv);
                        }
                    }
                }
            }
            
            newContinuationToken = response.ContinuationToken;
            Console.WriteLine(response.Count);
            Console.WriteLine(filter.PageSize);
            if (response.Count >= filter.PageSize)
            {
                break;
            }
        }

        return new GetUserConversations(userConversations, newContinuationToken);
    }
    
    

    private ConversationParticipants ToConversation(string id, string participant)
    {
        return new ConversationParticipants(
            id: id+"_"+participant,
            partitionKey: id,
            Participant: participant
        );
    }
    
    // private static IEnumerable<ConversationResponse> ToConversations(IEnumerable<ConversationParticipants> conversations, long timestamp)
    // {
    //     return conversations.Select(conv => new ConversationResponse(
    //         conv.partitionKey,
    //         conv.Profile,
    //         timestamp)
    //     );
    // }
}
