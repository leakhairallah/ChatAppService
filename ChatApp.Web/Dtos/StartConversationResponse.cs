using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record StartConversationResponse(
    [Required] string Id,
    [Required] long CreatedUnixTime);