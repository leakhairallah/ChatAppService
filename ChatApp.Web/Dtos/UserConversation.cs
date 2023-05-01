using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record UserConversation(
    List<GetMessageResponse> Messages,
    string? NextUri
);