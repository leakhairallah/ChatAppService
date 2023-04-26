using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record StartConversation(
    [Required] SendMessageRequest FirstMessage,
    [Required] string[] Participants
    );