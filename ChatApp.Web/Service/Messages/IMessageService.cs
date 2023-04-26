using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Paginator;

namespace ChatApp.Web.Service.Messages;

public interface IMessageService
{
    Task EnqueueSendMessage(string conversationId, SendMessageRequest msg);
    Task<UploadMessageResponse?> PostMessageToConversation(string conversationId, SendMessageRequest msg, long datetime);
    Task<UserConversation?> GetMessageFromConversation(string conversationId, PaginationFilter filter, HttpRequest request);
}