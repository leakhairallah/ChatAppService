using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;

namespace ChatApp.Web.Service.Messages;

public interface IMessageService
{
    Task EnqueueSendMessage(PostMessage msg);
    Task<UploadMessageResponse?> PostMessageToConversation(PostMessage msg, long datetime);
    Task<UserConversation?> GetMessageFromConversation(string conversationId, PaginationFilter filter, HttpRequest request);
}