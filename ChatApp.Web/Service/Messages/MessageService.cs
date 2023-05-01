using System.Web;
using Azure;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Messages;
using ChatApp.Web.Service.ServiceBus;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Storage.Conversations;
using Microsoft.Extensions.Logging.Abstractions;

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

    public async Task EnqueueSendMessage(string conversationId, SendMessageRequest msg)
    {
        var response = await _messageStore.MessageConflictCheck(conversationId, msg);

        if (response.MessageExists)
        {
            throw new Exception("Message already exists.");
        }
        await _sendMessageServiceBusPublisher.Send(conversationId, msg);
        
    }

    public async Task<UploadMessageResponse?> PostMessageToConversation(string conversationId, SendMessageRequest msg, long datetime)
    {
        var response = await _messageStore.PostMessageToConversation(conversationId, msg, datetime);

        if (response != null)
        {
            await _conversationStore.UpdateConversation(conversationId, response.timestamp);
        }

        return response;
    }

    public async Task<UserConversation?> GetMessageFromConversation(string conversationId, PaginationFilter filter, HttpRequest request)
    {
        
        var conversation = await _messageStore.GetMessageFromConversation(conversationId, filter);
        
        if (conversation != null)
        {
            if (String.IsNullOrEmpty(conversation.continuationToken))
            {
                return new UserConversation(conversation.Messages, null);
            }
            return new UserConversation(conversation.Messages,
                GetConversationMessagesApiNextUri(request, conversationId, filter.limit,
                    filter.lastSeenMessageTime, conversation.continuationToken));
        }
        
        return null;

    }
    
    private static string GetConversationMessagesApiNextUri(HttpRequest request, string conversationId, int limit, long lastSeenMessageTime, string continuationToken)
    {
        UriBuilder nextUri = new UriBuilder();
        nextUri.Scheme = request.Scheme;
        nextUri.Host = request.Host.Host;
        if (request.Host.Port.HasValue)
        {
            nextUri.Port = request.Host.Port.Value;
        }
        nextUri.Path = request.Path.ToString();
        nextUri.Query = $"conversationId={conversationId}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={HttpUtility.UrlEncode(continuationToken)}";
        return nextUri.Uri.ToString();
    }
}