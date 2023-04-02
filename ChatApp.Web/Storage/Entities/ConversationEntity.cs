namespace ChatApp.Web.Storage.Entities;

public record ConversationEntity(
    string id,
    string partitionKey,
    long timestamp
);

