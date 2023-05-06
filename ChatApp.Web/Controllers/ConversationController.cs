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
        }

    }

    [HttpGet]
    public async Task<ActionResult<string>> GetConversations(string username, [FromQuery] PaginationFilterConversation filter){
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