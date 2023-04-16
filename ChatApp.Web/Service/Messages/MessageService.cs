using System.Web;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Messages;
using ChatApp.Web.Service.ServiceBus;
using ChatApp.Web.Service.Paginator;
using Newtonsoft.Json;


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
        UriBuilder nextUri = new UriBuilder(request.Scheme, request.Host.Host);
        nextUri.Path = request.Path;
        nextUri.Query = $"continuationToken={HttpUtility.UrlEncode(continuationToken)}&lastSeenMessageTime={lastSeenMessageTime}&conversationId={conversationId}&limit={limit}";
        return nextUri.Uri.ToString();
    }
}