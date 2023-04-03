using System.Net;
using ChatApp.Web.Storage;

namespace ChatApp.Web.Service;

public class ConversationsService : IConversationsService
{
    private readonly IConversationStore _conversationStore;
    private readonly IConversationParticipantsStore _conversationParticipantsStore;

    public ConversationsService(IConversationStore conversationStore, IConversationParticipantsStore conversationParticipantsStore)
    {
        _conversationStore = conversationStore;
        _conversationParticipantsStore = conversationParticipantsStore;
    }
    
    public async Task<string> AddConversation(string username1, string username2, long time)
    {
        //TODO: check is username1 and username2 already exist 
        if (time > DateTime.UtcNow.Millisecond)
        {
            throw new ArgumentException("Time Exceeded: {time}");
        }
        var conversationId = await _conversationParticipantsStore.AddConversation(username1, username2);
        var response = await _conversationStore.AddConversation(conversationId, time);
        
        //TODO: handle errors; what if response is not 200 ok? etc.. 

        return conversationId;
    }

    public async Task<HttpStatusCode> UpdateConversation(string conversationId, long time)
    {
        if (time > DateTime.UtcNow.Millisecond)
        {
            throw new ArgumentException("Time Exceeded: {time}");
        }
        return await _conversationStore.UpdateConversation(conversationId, time);
    }
}