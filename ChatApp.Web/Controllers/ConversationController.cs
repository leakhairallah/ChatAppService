using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Conversations;
using ChatApp.Web.Service.Paginator;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ConversationsController : ControllerBase
{
    private readonly IConversationsService _conversationsService;
    private readonly ILogger<ConversationsController> _logger;
    
    public ConversationsController(IConversationsService conversationsService, ILogger<ConversationsController> logger)
    {
        _conversationsService = conversationsService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<string>> StartConversation([FromBody] StartConversation firstConversation)
    {
        var response = 
            await _conversationsService.AddConversation(firstConversation);

        if (response == null)
        {
            return Conflict($"Failed to create conversation with {firstConversation.Participants[0]} and {firstConversation.Participants[1]}");
        }

        return Ok(response);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<string>> GetConversations(string username, [FromQuery] PaginationFilter filter){
        using (_logger.BeginScope("Fetching conversations of user {username}", username))
        {
            var request = HttpContext.Request;
            var userConversations = await _conversationsService.GetUserConversations(username, filter, request);

            if (userConversations == null)
            {
                return NotFound("There was error while trying to get conversations.");
            } 
            
            return Ok(userConversations);
        }
    }

}