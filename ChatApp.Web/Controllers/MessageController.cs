using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Messages;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Service.Paginator;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ChatApp.Web.Controllers;

//TODO: fix return messages; check if you checked for every error

[ApiController]
[Route("[controller]")]

public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessageController> _logger;

    public MessageController(
        IMessageService messageService, 
        ILogger<MessageController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    [HttpGet("{conversationId}")]
    public async Task<ActionResult<UserConversation>> GetMessageFromConversation(
        string conversationId,
        [FromQuery] PaginationFilter filter)
    {
        using (_logger.BeginScope("Fetching messages from conversation with {id}", conversationId))
        {
            var messagesFromConversation = await _messageService.GetMessageFromConversation(conversationId, filter);

            if (messagesFromConversation == null)
            {
                return NotFound("There was error while trying to get messages.");
            } 
            
            return Ok(messagesFromConversation);
        }
    }

    [HttpPost]
    public async Task<ActionResult<UploadMessageResponse>> PostMessageToConversation(PostMessage msg)
    {
        using (_logger.BeginScope("Queuing message"))
        {
            await _messageService.EnqueueSendMessage(msg);

            return Ok("Successful!");
        }
    }
}