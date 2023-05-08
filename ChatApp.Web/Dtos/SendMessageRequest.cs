using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record SendMessageRequest(
    string Id,
    [Required] string SenderUsername,
    [Required] string Text
    );