using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record GetMessageResponse(
    [Required] string Text,
    [Required] string SenderUsername,
    [Required] long UnixTime);