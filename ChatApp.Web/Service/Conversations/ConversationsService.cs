using System.Data;
using System.Net;
using System.Web;
using ChatApp.Web.Dtos;
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
        Console.WriteLine("before get profile");
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
        Console.WriteLine("hello");
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

    public async Task<GetUserConversationsResponse?> GetUserConversations(string username, PaginationFilter filter, HttpRequest request)
    {
        var userConv =  await _conversationParticipantsStore.GetConversations(username, filter);
        
        return new GetUserConversationsResponse(userConv.Conversations,
            GetUserConversationsApiNextUri(request, username, filter.PageSize,
                filter.LastSeenMessageTime, userConv.ContinuationToken));

    }
    
    private static string GetUserConversationsApiNextUri(HttpRequest request, string username, int limit, long lastSeenMessageTime, string continuationToken)
    {
        UriBuilder nextUri = new UriBuilder();
        nextUri.Path = request.Path;
        nextUri.Query = $"username={username}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={HttpUtility.UrlEncode(continuationToken)}";
        return nextUri.Uri.ToString();
    }
}