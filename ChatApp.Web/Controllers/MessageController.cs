using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Messages;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Service.Paginator;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using ChatApp.Web.Exceptions;

namespace ChatApp.Web.Controllers;

//TODO: fix return messages; check if you checked for every error

[ApiController]
[Route("api/conversations/{conversationId}/[Controller]")]

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
        using (_logger.BeginScope("Fetching messages from conversation with {id}...", conversationId))
        {
            var request = HttpContext.Request;
            
            _logger.LogInformation("Calling message service...");
            var messagesFromConversation =
                await _messageService.GetMessageFromConversation(conversationId, filter, request);
            
            
            if (messagesFromConversation == null)
            {
                return NotFound("Messages from conversation not found.");
            }

            return Ok(messagesFromConversation);
        }
    }

    [HttpPost]
    public async Task<ActionResult<UploadMessageResponse>> PostMessageToConversation(string conversationId, [FromBody] SendMessageRequest msg)
    {
        using (_logger.BeginScope("Queuing message..."))
        {
            try
            {
                _logger.LogInformation("Calling message service...");
                await _messageService.EnqueueSendMessage(conversationId, msg);
                return CreatedAtAction(nameof(PostMessageToConversation), new { senderUsername = msg.SenderUsername },
                    msg);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}