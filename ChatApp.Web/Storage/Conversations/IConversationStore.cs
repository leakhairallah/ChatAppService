using System.Net;
using ChatApp.Web.Storage.Entities;

namespace ChatApp.Web.Storage.Conversations;

public interface IConversationStore
{
    public Task<string?> AddConversation(string conversationId, long time);
    public Task<HttpStatusCode> UpdateConversation(string conversationId, long time);
    public Task<ConversationEntity?> GetConversation(string id);
}