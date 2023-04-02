namespace ChatApp.Web.Storage.Entities;

public record MessageEntity(
    string partitionKey,
    string MessageId,
    string SenderUsername,
    string Content,
    long Timestamp);