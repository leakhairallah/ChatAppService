using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record StartConversation(
    [Required] string[] Participants,
    [Required] SendMessageRequest FirstMessage
    );