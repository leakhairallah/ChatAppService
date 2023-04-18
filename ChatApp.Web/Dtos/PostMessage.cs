using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record PostMessage(
     string ConversationId,
    [Required] string Content,
    [Required] string SenderUsername);