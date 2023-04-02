using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record Conversation(
    [Required] string ConversationId,
    [Required] string Participant1,
    [Required] string Participant2,
    [Required] string ModifiedTime);