using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record ConversationResponse(
    [Required] string Id,
    [Required] long LastModifiedUnixTime,
    [Required] Profile Recipient);
