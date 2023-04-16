using System.Net;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Conversations;
using ChatApp.Web.Storage.Conversations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ConversationController : ControllerBase
{
    private readonly IConversationsService _conversationsService;
    
    public ConversationController(IConversationsService conversationsService)
    {
        _conversationsService = conversationsService;
    }

    [HttpPost]
    public async Task<ActionResult<string>> StartConversation(string user1, string user2)
    {
        var response = 
            await _conversationsService.AddConversation(user1, user2, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        if (response == null)
        {
            return Conflict($"Failed to create conversation with {user1} and {user2}");
        }

        return Ok(response);
    }

    // GET api/conversations?username={username}&continuationToken={continuationToken}&limit={limit}&lastSeenConversationTime={lastSeenMessageTime}

}