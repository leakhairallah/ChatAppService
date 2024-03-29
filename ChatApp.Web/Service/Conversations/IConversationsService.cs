using System.Net;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;

namespace ChatApp.Web.Service.Conversations;

public interface IConversationsService
{
    public Task<StartConversationResponse> AddConversation(StartConversation conversation);

    public Task<HttpStatusCode> UpdateConversation(string conversationId, long time);
    
    Task<GetUserConversationsResponse?> GetUserConversations(string username, PaginationFilterConversation filter, HttpRequest request);
}