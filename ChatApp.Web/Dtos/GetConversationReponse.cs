namespace ChatApp.Web.Dtos;

public record GetConversationResponse(
    List<GetMessageResponse> Messages,
    string continuationToken
);