using System.Net;
using System.Web;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
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
    private readonly ILogger<MessageService> _logger;
    
    public MessageService(
        IMessageStore messageStore,
        IConversationStore conversationStore,
        ISendMessageServiceBusPublisher sendMessageServiceBusPublisher,
        ILogger<MessageService> logger)
    {
        _messageStore = messageStore;
        _sendMessageServiceBusPublisher = sendMessageServiceBusPublisher;
        _conversationStore = conversationStore;
        _logger = logger;
    }

    public async Task EnqueueSendMessage(string conversationId, SendMessageRequest msg)
    {
        _logger.LogInformation("Checking for conflict...");
        var response = await _messageStore.MessageConflictCheck(conversationId, msg);
        
        if (response.MessageExists)
        {
            throw new ConflictException("already exists.", "Message", HttpStatusCode.Conflict);
        }
        
        _logger.LogInformation("Sending message to publisher...");
        await _sendMessageServiceBusPublisher.Send(conversationId, msg);
    }

    public async Task<UploadMessageResponse?> PostMessageToConversation(string conversationId, SendMessageRequest msg, long datetime)
    {
        _logger.LogInformation("Adding message to conversation with id {id}...", conversationId);
        var response = await _messageStore.PostMessageToConversation(conversationId, msg, datetime);

        if (response != null)
        {
            _logger.LogInformation("Updating last modified time of conversation: {time}...", response.timestamp);
            await _conversationStore.UpdateConversation(conversationId, response.timestamp);
            
            return response;
        }

        return null;
    }

    public async Task<UserConversation?> GetMessageFromConversation(string conversationId, PaginationFilter filter, HttpRequest request)
    {
        _logger.LogInformation("Getting messages from conversation with id {id}...", conversationId);
        var conversation = await _messageStore.GetMessageFromConversation(conversationId, filter);
        
        if (conversation != null)
        {
            if (String.IsNullOrEmpty(conversation.continuationToken))
            {
                return new UserConversation(conversation.Messages, null);
            }
            _logger.LogInformation("Building next URI...");
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