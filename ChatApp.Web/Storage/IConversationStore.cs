using System.Net;

namespace ChatApp.Web.Storage;

public interface IConversationStore
{
    public Task<HttpStatusCode> AddConversation(string conversationId, long time);
    public Task<HttpStatusCode> UpdateConversation(string conversationId, long time);
}