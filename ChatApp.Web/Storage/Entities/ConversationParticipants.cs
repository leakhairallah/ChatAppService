namespace ChatApp.Web.Storage.Entities;

public record ConversationParticipants(
    string id, 
    string partitionKey,
    string Participant
);