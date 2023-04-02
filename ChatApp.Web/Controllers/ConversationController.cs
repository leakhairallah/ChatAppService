using System.Net;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service;
using ChatApp.Web.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Web.Controllers;

//TODO: use service layer in this controller
//TODO: fix all of this controller since it was only used for testing purposes

[ApiController]
[Route("[controller]")]

public class ConversationController : ControllerBase
{

    private readonly IConversationStore _conversationStore;

    public ConversationController(IConversationStore conversationStore)
    {
        _conversationStore = conversationStore;
    }

    [HttpPost]
    public async Task<ActionResult<string>> StartConversation(string conversationId)
    {
        var response =
            await _conversationStore.AddConversation(conversationId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        if (response == HttpStatusCode.Conflict)
        {
            return Conflict("conflict");
        }

        return Ok(conversationId);
    }

    // [HttpPut("{conversationId}")]
    // public async Task<ActionResult<UploadMessageResponse>> UpdateConversation(Message msg)
    // {
    //     
    // }
}