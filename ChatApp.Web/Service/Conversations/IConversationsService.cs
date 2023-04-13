using System.Net;

namespace ChatApp.Web.Service.Conversations;

public interface IConversationsService
{
    public Task<string> AddConversation(string username1, string username2, long time);

    public Task<HttpStatusCode> UpdateConversation(string conversationId, long time);
    
    // TODO: Delete Conversation
}