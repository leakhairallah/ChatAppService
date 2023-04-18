using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record ConversationResponse(
    [Required] string ConversationId,
    [Required] string participant,
    [Required] long TimeStamp);