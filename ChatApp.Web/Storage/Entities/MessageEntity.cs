namespace ChatApp.Web.Storage.Entities;

public record MessageEntity(
    string partitionKey,
    string id,
    string SenderUsername,
    string Content,
    long Timestamp);