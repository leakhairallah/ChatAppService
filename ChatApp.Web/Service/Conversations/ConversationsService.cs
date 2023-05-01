using System.Data;
using System.Net;
using System.Web;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Messages;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Service.Profiles;
using ChatApp.Web.Storage.Conversations;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.VisualBasic.CompilerServices;

namespace ChatApp.Web.Service.Conversations;

public class ConversationsService : IConversationsService
{
    private readonly IConversationStore _conversationStore;
    private readonly IConversationParticipantsStore _conversationParticipantsStore;
    private readonly IProfileService _profileService;
    private readonly IMessageService _messageService;

    
    public ConversationsService(
        IConversationStore conversationStore, 
        IConversationParticipantsStore conversationParticipantsStore, 
        IProfileService profileService, 
        IMessageService messageService)
    {
        _conversationStore = conversationStore;
        _conversationParticipantsStore = conversationParticipantsStore;
        _profileService = profileService;
        _messageService = messageService;
    }
    
    public async Task<StartConversationResponse> AddConversation(StartConversation conversation)
    {
        if (conversation.Participants.Length != 2)
        {
            throw new ArgumentOutOfRangeException("Invalid input, need 2 usernames");
        }
        if (string.IsNullOrWhiteSpace(conversation.Participants[0]) ||
            string.IsNullOrWhiteSpace(conversation.Participants[1]) ||
            string.IsNullOrWhiteSpace(conversation.FirstMessage.SenderUsername) ||
            string.IsNullOrWhiteSpace(conversation.FirstMessage.Text))
        {
            throw new ArgumentNullException($"Invalid input");
        }

        try
        {
            await _profileService.GetProfile(conversation.Participants[0]);
        }
        catch (Exception)
        {
            throw new ArgumentException($"A User with username {conversation.Participants[0]} was not found");   
        }
        try
        {
            await _profileService.GetProfile(conversation.Participants[1]);
        }
        catch (Exception)
        {
            throw new ArgumentException($"A User with username {conversation.Participants[1]} was not found");   
        }

        string conversationId = conversation.Participants[0] + "_" + conversation.Participants[1];
        
        var datetime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var createResponse = await _conversationStore.AddConversation(conversationId, datetime);
        
        if (createResponse == conversationId)
        {
            
            var messageResponse = await _messageService.PostMessageToConversation(conversationId, new SendMessageRequest(conversationId, conversation.FirstMessage.SenderUsername, conversation.FirstMessage.Text), datetime);
            
            var response = await _conversationParticipantsStore.AddConversation(conversation.Participants[0], conversation.Participants[1], conversationId);
            if (response == conversationId)
            {
                return new StartConversationResponse(conversationId, datetime);
            }
            else
            {
                throw new Exception($"Error creating conversation");
                
            }
        }
        else
        {
            throw new Exception($"Error creating conversation");
        }
        
    }

    public async Task<HttpStatusCode> UpdateConversation(string conversationId, long time)
    {
        if (string.IsNullOrWhiteSpace(conversationId) || time.IsNull())
        {
            throw new ArgumentException($"Invalid input {conversationId}", nameof(conversationId));
        }

        if (time > DateTime.UtcNow.Millisecond)
        {
            throw new ArgumentException($"Time Exceeded: {time}");
        }
        
        var response = _conversationStore.GetConversation(conversationId);
        
        
        return await _conversationStore.UpdateConversation(conversationId, time);
    }

    public async Task<GetUserConversationsResponse?> GetUserConversations(string username, PaginationFilterConversation filter, HttpRequest request)
    {
        var userConv =  await _conversationParticipantsStore.GetConversations(username, filter);

        if (String.IsNullOrEmpty(userConv.ContinuationToken))
        {
            return new GetUserConversationsResponse(userConv.Conversations, null);
        }

        return new GetUserConversationsResponse(userConv.Conversations,
            GetUserConversationsApiNextUri(request, username, filter.limit,
                filter.lastSeenConversationTime, userConv.ContinuationToken));
        
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