using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Messages;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Service.Paginator;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;

namespace ChatApp.Web.Controllers;

//TODO: fix return messages; check if you checked for every error

[ApiController]
[Route("api/conversation/{conversationId}/[Controller]")]

public class messagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<messagesController> _logger;

    public messagesController(
        IMessageService messageService, 
        ILogger<messagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<UserConversation>> GetMessageFromConversation(
        string conversationId,
        [FromQuery] PaginationFilter filter)
    {
        using (_logger.BeginScope("Fetching messages from conversation with {id}", conversationId))
        {
            var request = HttpContext.Request;
            var messagesFromConversation = await _messageService.GetMessageFromConversation(conversationId, filter, request);

            if (messagesFromConversation == null)
            {
                return NotFound("There was an error while trying to get messages.");
            }

            return Ok(messagesFromConversation);
        }
    }

    [HttpPost]
    public async Task<ActionResult<UploadMessageResponse>> PostMessageToConversation(string conversationId, [FromBody] SendMessageRequest msg)
    {
        using (_logger.BeginScope("Queuing message"))
        {
            try
            {
                await _messageService.EnqueueSendMessage(conversationId, msg);
                return Ok("Successful!");
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}