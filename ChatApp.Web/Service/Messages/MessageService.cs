using System.Web;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Messages;
using ChatApp.Web.Service.ServiceBus;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Storage.Conversations;

namespace ChatApp.Web.Service.Messages;

public class MessageService : IMessageService
{
    private readonly IMessageStore _messageStore;
    private readonly IConversationStore _conversationStore;
    private readonly ISendMessageServiceBusPublisher _sendMessageServiceBusPublisher;
    
    public MessageService(
        IMessageStore messageStore,
        IConversationStore conversationStore,
        ISendMessageServiceBusPublisher sendMessageServiceBusPublisher)
    {
        _messageStore = messageStore;
        _sendMessageServiceBusPublisher = sendMessageServiceBusPublisher;
        _conversationStore = conversationStore;
    }
    
    public async Task EnqueueSendMessage(PostMessage msg)
    {
        await _sendMessageServiceBusPublisher.Send(msg);
    }
    
    public async Task<UploadMessageResponse?> PostMessageToConversation(PostMessage msg)
    {
        var response = await _messageStore.PostMessageToConversation(msg);
        if (response != null)
        {
            await _conversationStore.UpdateConversation(msg.ConversationId, response.timestamp);
        }

        return response;
    }

    public async Task<UserConversation?> GetMessageFromConversation(string conversationId, PaginationFilter filter, HttpRequest request)
    {
        
        var conversation = await _messageStore.GetMessageFromConversation(conversationId, filter);

        if (conversation != null)
        {
            return new UserConversation(conversation.Messages,
                GetConversationMessagesApiNextUri(request, conversationId, filter.PageSize,
                    filter.LastSeenMessageTime, conversation.continuationToken));
        }
        
        return null;

    }
    
    private static string GetConversationMessagesApiNextUri(HttpRequest request, string conversationId, int limit, long lastSeenMessageTime, string continuationToken)
    {
        UriBuilder nextUri = new UriBuilder();
        nextUri.Path = request.Path;
        nextUri.Query = $"continuationToken={HttpUtility.UrlEncode(continuationToken)}&lastSeenMessageTime={lastSeenMessageTime}&conversationId={conversationId}&limit={limit}";
        return nextUri.Uri.ToString();
    }
}