using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record SendMessageRequest(
    [Required] string Id,
    [Required] string Text,
    [Required] string SenderUsername);