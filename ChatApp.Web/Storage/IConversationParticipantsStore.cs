using System.Net;

namespace ChatApp.Web.Storage;

public interface IConversationParticipantsStore
{
    public Task<string> AddConversation(string participant1, string participant2);
    // public Task<HttpStatusCode> DeleteConversation(string conversationId);
}