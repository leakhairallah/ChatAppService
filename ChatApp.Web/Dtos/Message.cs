using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record Message(
    [Required] string ConversationId,
    [Required] string Content,
    [Required] string SenderUsername);