using System.Net;
using ChatApp.Web.Service.Profiles;
using ChatApp.Web.Storage.Conversations;
using ChatApp.Web.Storage.Entities;
using ChatApp.Web.Storage.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;

namespace ChatApp.Web.Service.Conversations;

public class ConversationsService : IConversationsService
{
    private readonly IConversationStore _conversationStore;
    private readonly IConversationParticipantsStore _conversationParticipantsStore;
    private readonly IProfileService _profileService;
    
    public ConversationsService(IConversationStore conversationStore, IConversationParticipantsStore conversationParticipantsStore, IProfileService profileService)
    {
        _conversationStore = conversationStore;
        _conversationParticipantsStore = conversationParticipantsStore;
        _profileService = profileService;
    }
    
    public async Task<string> AddConversation(string username1, string username2, long time)
    {
        if (string.IsNullOrWhiteSpace(username1) ||
            string.IsNullOrWhiteSpace(username2))
        {
            throw new ArgumentException($"Invalid input for {username1} or {username2}");
        }
        
        var profile1 = _profileService.GetProfile(username1);
        var profile2 = _profileService.GetProfile(username2);
        if (profile1 == null)
        {
            throw new ArgumentException($"A User with username {username1} was not found");
        }
        if (profile2 == null)
        {
            throw new ArgumentException($"A User with username {username2} was not found");
        }
        
        if (time > DateTimeOffset.UtcNow.ToUnixTimeSeconds() || time == null)
        {
            throw new ArgumentException("Invalid Time: {time}");
        }
        
        var conversationId = await _conversationParticipantsStore.AddConversation(username1, username2);

        var response = await _conversationStore.AddConversation(conversationId, time);
        
        return conversationId;
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
}