using System.Net;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Storage.Entities;

namespace ChatApp.Web.Storage.Conversations;

public interface IConversationParticipantsStore
{
    public Task<string> AddConversation(string participant1, string participant2, string id);
    public Task<GetUserConversations> GetConversations(string participant, PaginationFilter filter);
}