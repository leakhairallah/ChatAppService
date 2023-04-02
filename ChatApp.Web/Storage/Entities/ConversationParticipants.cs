namespace ChatApp.Web.Storage.Entities;

public record ConversationParticipants(
    string PartitionKey, //conversation id
    string Participant
);