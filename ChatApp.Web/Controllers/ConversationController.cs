using System.Net;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
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
        using (_logger.BeginScope("Creating conversation between {participant1} and {participant2}...", firstConversation.Participants[0], firstConversation.Participants[1]))
        {
            try
            {
                _logger.LogInformation("Calling conversation service...");
                var response =
                    await _conversationsService.AddConversation(firstConversation);
                
                return Created("Successful!", response);
            }
            catch (InvalidConversationException e) 
            {
                return BadRequest(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            return Ok(
                $"Failed to create conversation with {firstConversation.Participants[0]} and {firstConversation.Participants[1]}");
            }

    }

    [HttpGet]
    public async Task<ActionResult<string>> GetConversations(string username, [FromBody] PaginationFilterConversation filter){
        using (_logger.BeginScope("Calling conversation service..."))
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