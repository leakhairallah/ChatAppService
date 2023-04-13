using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;

namespace ChatApp.Web.Service.Messages;

public interface IMessageService
{
    Task EnqueueSendMessage(PostMessage msg);
    Task<UploadMessageResponse?> PostMessageToConversation(PostMessage msg);

    Task<UserConversation?> GetMessageFromConversation(string conversationId, PaginationFilter filter);
}