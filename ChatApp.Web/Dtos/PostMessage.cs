using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record PostMessage(
    [Required] string ConversationId,
    [Required] string Content,
    [Required] string SenderUsername);