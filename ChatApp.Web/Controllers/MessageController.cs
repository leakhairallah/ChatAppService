using ChatApp.Web.Dtos;
using ChatApp.Web.Service;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

//TODO: fix return messages; check if you checked for every error

[ApiController]
[Route("[controller]")]

public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("{conversationId}")]
    public async Task<ActionResult<UserConversation>> GetMessageFromConversation(string conversationId)
    {
        var conversation = await _messageService.GetMessageFromConversation(conversationId);
        if (conversation == null)
        {
            return NotFound($"A conversation with id {conversationId} was not found");
        }

        return Ok(conversation);
    }

    [HttpPost]
    public async Task<ActionResult<UploadMessageResponse>> PostMessageToConversation(Message msg)
    {
        var newMessage = await _messageService.PostMessageToConversation(msg);

        if (newMessage == null)
        {
            return NotFound($"Tried posting a message to a non existing conversation");
        }
        return Ok(new UploadMessageResponse(newMessage.timestamp));
    }
}