using System.Net;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;

namespace ChatApp.Web.Service.Conversations;

public interface IConversationsService
{
    public Task<string> AddConversation(string username1, string username2,  PostMessage message);

    public Task<HttpStatusCode> UpdateConversation(string conversationId, long time);
    
    Task<GetUserConversations?> GetUserConversations(string username, PaginationFilter filter, HttpRequest request);
}