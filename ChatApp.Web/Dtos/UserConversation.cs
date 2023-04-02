using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record UserConversation(
    List<Message> Messages
    // string NextUri to figure out later
    );