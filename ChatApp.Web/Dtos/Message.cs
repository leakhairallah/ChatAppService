using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record Message(
    [Required] string ConversationId,
    [Required] string MessageId, 
    [Required] string Content,
    // [Required] string SenderId,
    // [Required] string ReceiverId,
    [Required] string DateTime);