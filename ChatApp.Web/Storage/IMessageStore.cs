using ChatApp.Web.Dtos;

namespace ChatApp.Web.Storage;

public interface IMessageStore
{
    Task<UploadMessageResponse?> PostMessageToConversation(Message msg);

    Task<UserConversation?> GetMessageFromConversation(String conversationId);
}