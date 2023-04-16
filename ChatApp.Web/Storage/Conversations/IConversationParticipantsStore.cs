using System.Net;
using ChatApp.Web.Storage.Entities;

namespace ChatApp.Web.Storage.Conversations;

public interface IConversationParticipantsStore
{
    public Task<string> AddConversation(string participant1, string participant2);
    // public Task<HttpStatusCode> DeleteConversation(string conversationId);
}