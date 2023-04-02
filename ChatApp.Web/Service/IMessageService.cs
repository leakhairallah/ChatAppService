using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service;
//TODO: add service bus 
public interface IMessageService
{
    Task<UploadMessageResponse?> PostMessageToConversation(Message msg);

    Task<UserConversation?> GetMessageFromConversation(string conversationId);
}