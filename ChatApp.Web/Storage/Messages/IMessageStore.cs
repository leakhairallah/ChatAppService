using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;

namespace ChatApp.Web.Storage.Messages;

public interface IMessageStore
{
    Task<UploadMessageResponse?> PostMessageToConversation(PostMessage msg, long datetime);

    Task<GetConversationResponse?> GetMessageFromConversation(String conversationId, PaginationFilter filter);
}