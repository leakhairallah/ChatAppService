namespace ChatApp.Web.Storage.Entities;

public record ProfileEntity(string PartitionKey, string Id, string FirstName, string LastName, string ProfilePictureId);