namespace ChatApp.Web.Dtos;

public record GetUserConversations(
    List<ConversationResponse> Conversations,
    string ContinuationToken);