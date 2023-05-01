namespace ChatApp.Web.Dtos;

public record GetUserConversationsResponse(
    List<ConversationResponse> Conversations,
    string? NextUri);