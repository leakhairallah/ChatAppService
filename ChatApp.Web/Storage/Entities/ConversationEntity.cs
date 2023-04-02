namespace ChatApp.Web.Storage.Entities;

public record ConversationEntity(
    string PartitionKey, 
    long ModifiedUnixTime);