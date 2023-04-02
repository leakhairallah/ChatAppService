using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service;

public interface IMessageService
{
    Task<UploadMessageResponse?> PostMessageToConversation(Message msg);

    Task<UserConversation?> GetMessageFromConversation();
}