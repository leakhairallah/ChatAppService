using System.Net;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Messages;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Service.Profiles;
using ChatApp.Web.Storage.Conversations;
using ChatApp.Web.Storage.Entities;
using ChatApp.Web.Storage.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
    
    public async Task<string> AddConversation(StartConversation conversation)
    {
        if (conversation.Participants.Length != 2)
        {
            throw new ArgumentException("Invalid input, need 2 usernames");
        }
        if (string.IsNullOrWhiteSpace(conversation.Participants[0]) ||
            string.IsNullOrWhiteSpace(conversation.Participants[1]) ||
            string.IsNullOrWhiteSpace(conversation.FirstMessage.SenderUsername) ||
            string.IsNullOrWhiteSpace(conversation.FirstMessage.Text))
        {
            throw new ArgumentException($"Invalid input");
        }
        
        var profile1 = _profileService.GetProfile(conversation.Participants[0]);
        var profile2 = _profileService.GetProfile(conversation.Participants[1]);
        if (profile1 == null)
        {
            throw new ArgumentException($"A User with username {conversation.Participants[0]} was not found");
        }
        if (profile2 == null)
        {
            throw new ArgumentException($"A User with username {conversation.Participants[1]} was not found");
        }
        
        // if (time > DateTimeOffset.UtcNow.ToUnixTimeSeconds() || time == null)
        // {
        //     throw new ArgumentException("Invalid Time: {time}");
        // }

        string conversationId = conversation.Participants[0] + "_" + conversation.Participants[1];
        
        var datetime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var createResponse = await _conversationStore.AddConversation(conversationId, datetime);

        if (createResponse == conversationId)
        {
            var messageResponse = await _messageService.PostMessageToConversation(conversationId, conversation.FirstMessage, datetime);
            var response = await _conversationParticipantsStore.AddConversation(conversation.Participants[0], conversation.Participants[1], conversationId);
            if (response == conversationId)
            {
                return conversationId;
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

    public async Task<GetUserConversations?> GetUserConversations(string username, PaginationFilter filter, HttpRequest request)
    {
        return await _conversationParticipantsStore.GetConversations(username, filter);;
    }
}