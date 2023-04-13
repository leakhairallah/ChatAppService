using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Messages;
using ChatApp.Web.Service.ServiceBus;
using ChatApp.Web.Service.Paginator;

namespace ChatApp.Web.Service.Messages;

public class MessageService : IMessageService
{
    private readonly IMessageStore _messageStore;
    private readonly ISendMessageServiceBusPublisher _sendMessageServiceBusPublisher;
    
    public MessageService(
        IMessageStore messageStore,
        ISendMessageServiceBusPublisher sendMessageServiceBusPublisher)
    {
        _messageStore = messageStore;
        _sendMessageServiceBusPublisher = sendMessageServiceBusPublisher;
    }
    
    public async Task EnqueueSendMessage(PostMessage msg)
    {
        await _sendMessageServiceBusPublisher.Send(msg);
    }
    
    public async Task<UploadMessageResponse?> PostMessageToConversation(PostMessage msg)
    {
        return await _messageStore.PostMessageToConversation(msg);
    }

    public async Task<UserConversation?> GetMessageFromConversation(string conversationId, PaginationFilter filter)
    {
        
        var conversation = await _messageStore.GetMessageFromConversation(conversationId, filter);

        if (conversation != null)
        {
            if (string.IsNullOrWhiteSpace(conversation.continuationToken))
            {
                return new UserConversation(conversation.Messages,
                    GetConversationMessagesApiUrl(conversationId, filter.PageSize, filter.LastSeenMessageTime,
                        filter.ContinuationToken));
            }
        }
        
        return null;

    }
    
    private static string GetConversationMessagesApiUrl(string conversationId, int limit, long lastSeenMessageTime, string continuationToken)
    {
        return $"/Messages/{conversationId}/messages?&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={continuationToken}";
    }
}