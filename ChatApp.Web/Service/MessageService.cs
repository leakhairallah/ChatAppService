using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;

namespace ChatApp.Web.Service;

public class MessageService : IMessageService
{
    private readonly IMessageStore _messageStore;
    
    public MessageService(IMessageStore messageStore)
    {
        _messageStore = messageStore;
    }
    
    public async Task<UploadMessageResponse?> PostMessageToConversation(Message msg)
    {
        return await _messageStore.PostMessageToConversation(msg);
    }

    public async Task<UserConversation?> GetMessageFromConversation()
    {
        return null;
    }
}