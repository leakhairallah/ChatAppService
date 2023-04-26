using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record StartConversation(
    [Required] string[] participants,
    [Required] PostMessage FirstMessage
    );