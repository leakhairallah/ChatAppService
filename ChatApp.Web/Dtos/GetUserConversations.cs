namespace ChatApp.Web.Dtos;

public record GetUserConversations(
    List<ConversationResponse> conversations,
    string continuationToken);