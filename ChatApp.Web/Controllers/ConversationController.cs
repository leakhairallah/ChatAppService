using System.Net;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Conversations;
using ChatApp.Web.Service.Paginator;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]

public class conversationsController : ControllerBase
{
    private readonly IConversationsService _conversationsService;
    private readonly ILogger<conversationsController> _logger;
    
    public conversationsController(IConversationsService conversationsService, ILogger<conversationsController> logger)
    {
        _conversationsService = conversationsService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<string>> StartConversation([FromBody] StartConversation firstConversation)
    {
        try
        {
            var response =
                await _conversationsService.AddConversation(firstConversation);

            if (response == null)
            {
                return Conflict(
                    $"Failed to create conversation with {firstConversation.Participants[0]} and {firstConversation.Participants[1]}");
            }

            // 200
            return Created("", response);

        }
        catch (ArgumentNullException) //404
        {
            return BadRequest("Arguments cannot be null");
        }
        catch (ArgumentOutOfRangeException) //400
        {
            return BadRequest("There can only be two participants in a conversation");
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e);
            return NotFound("User not found");
        }
        catch (Exception e) //409
        {
            return Conflict("Conversation Already Exists");
        }

    }

    [HttpGet]
    public async Task<ActionResult<string>> GetConversations(string username, [FromQuery] PaginationFilterConversation filter){
        using (_logger.BeginScope("Fetching conversations of user {username}", username))
        {
            var request = HttpContext.Request;
            var userConversations = await _conversationsService.GetUserConversations(username, filter, request);
            
            Console.WriteLine("Controller filter: " + filter.lastSeenConversationTime);
            if (userConversations == null)
            {
                return NotFound("There was error while trying to get conversations.");
            } 
            
            return Ok(userConversations);
        }
    }

}