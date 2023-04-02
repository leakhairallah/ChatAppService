namespace ChatApp.Web.Storage.Entities;

public record ConversationEntity(
    string partitionKey,
    long timestamp
);