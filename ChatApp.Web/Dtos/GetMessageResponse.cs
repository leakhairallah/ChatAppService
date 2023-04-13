using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record GetMessageResponse(
    [Required] string Content,
    [Required] string SenderUsername,
    [Required] long TimeStamp);