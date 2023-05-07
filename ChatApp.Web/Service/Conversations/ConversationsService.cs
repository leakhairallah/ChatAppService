using System.Net;
using System.Web;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
using ChatApp.Web.Service.Messages;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Service.Profiles;
using ChatApp.Web.Storage.Conversations;
using Microsoft.Azure.Cosmos.Linq;

namespace ChatApp.Web.Service.Conversations;

public class ConversationsService : IConversationsService
{
    private readonly IConversationStore _conversationStore;
    private readonly IConversationParticipantsStore _conversationParticipantsStore;
    private readonly IProfileService _profileService;
    private readonly IMessageService _messageService;
    private readonly ILogger<ConversationsService> _logger;

    
    public ConversationsService(
        IConversationStore conversationStore, 
        IConversationParticipantsStore conversationParticipantsStore, 
        IProfileService profileService, 
        IMessageService messageService, 
        ILogger<ConversationsService> logger)
    {
        _conversationStore = conversationStore;
        _conversationParticipantsStore = conversationParticipantsStore;
        _profileService = profileService;
        _messageService = messageService;
        _logger = logger;
    }
    
    public async Task<StartConversationResponse> AddConversation(StartConversation conversation)
    {
        Console.WriteLine("Im in the service layer");
        using (_logger.BeginScope("Checking for exceptions..."))
        {
            if (conversation.Participants.Length != 2)
            {
                throw new InvalidConversationException("There can only be 2 participants in a conversation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrWhiteSpace(conversation.Participants[0]) ||
                string.IsNullOrWhiteSpace(conversation.Participants[1]) ||
                string.IsNullOrWhiteSpace(conversation.FirstMessage.SenderUsername) ||
                string.IsNullOrWhiteSpace(conversation.FirstMessage.Text))
            {
                throw new InvalidConversationException("Invalid input.", HttpStatusCode.BadRequest);
            }

            try
            {
                await _profileService.GetProfile(conversation.Participants[0]);
            }
            catch (NotFoundException e)
            {
                throw new NotFoundException($"with username {conversation.Participants[0]}", "User", HttpStatusCode.NotFound);
            }
            try
            {
                await _profileService.GetProfile(conversation.Participants[1]);
            }
            catch (NotFoundException e)
            {
                throw new NotFoundException($"with username {conversation.Participants[1]}", "User", HttpStatusCode.NotFound);
            }

            string conversationId = conversation.Participants[0] + "_" + conversation.Participants[1];
        
            var datetime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            _logger.LogInformation("Adding conversation to Conversation Store");
        
            var createResponse = await _conversationStore.AddConversation(conversationId, datetime);
        
            if (createResponse == conversationId)
            {
                _logger.LogInformation("Posting first message to conversation...");
                await _messageService.PostMessageToConversation(conversationId, new SendMessageRequest(conversationId, conversation.FirstMessage.SenderUsername, conversation.FirstMessage.Text), datetime);
            
                _logger.LogInformation("Adding participants to Conversation Participants Store...");
                var response = await _conversationParticipantsStore.AddConversation(conversation.Participants[0], conversation.Participants[1], conversationId);
                if (response == conversationId)
                {
                    return new StartConversationResponse(conversationId, datetime);
                }
            
                throw new Exception($"Error creating conversation.");
                
            }
            throw new Exception($"Error creating conversation.");
        }

        
    }

    public async Task<HttpStatusCode> UpdateConversation(string conversationId, long time)
    {
        using (_logger.BeginScope("Checking for exceptions..."))
        {
            if (string.IsNullOrWhiteSpace(conversationId) || time.IsNull())
            {
                throw new InvalidConversationException($"Invalid input {conversationId}", HttpStatusCode.BadRequest);
            }

            if (time > DateTime.UtcNow.Millisecond)
            {
                throw new ArgumentException($"Time Exceeded: {time}");
            }
            
            _logger.LogInformation("Calling Conversation Store...");
            await _conversationStore.GetConversation(conversationId);
            
            _logger.LogInformation("Updating last modified time of conversation: {time}...", time);
            return await _conversationStore.UpdateConversation(conversationId, time);
        }
    }

    public async Task<GetUserConversationsResponse?> GetUserConversations(string username, PaginationFilterConversation filter, HttpRequest request)
    {
        using (_logger.BeginScope("Calling Conversation Participants Store..."))
        {
            var userConv =  await _conversationParticipantsStore.GetConversations(username, filter);

            if (userConv == null)
            {
                return null;
            }

            if (String.IsNullOrEmpty(userConv.ContinuationToken))
            {
                return new GetUserConversationsResponse(userConv.Conversations, null);
            }
            
            _logger.LogInformation("Building next URI...");
            return new GetUserConversationsResponse(userConv.Conversations,
                GetUserConversationsApiNextUri(request, username, filter.limit,
                    filter.lastSeenConversationTime, userConv.ContinuationToken));
        }
        
    }
    
    private static string GetUserConversationsApiNextUri(HttpRequest request, string username, int limit, long lastSeenConversationTime, string continuationToken)
    {
        UriBuilder nextUri = new UriBuilder();
        nextUri.Scheme = request.Scheme;
        nextUri.Host = request.Host.Host;
        if (request.Host.Port.HasValue)
        {
            nextUri.Port = request.Host.Port.Value;
        }
        nextUri.Path = request.Path.ToString();
        nextUri.Query = $"username={username}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}&continuationToken={HttpUtility.UrlEncode(continuationToken)}";
        return nextUri.Uri.ToString();
    }
}