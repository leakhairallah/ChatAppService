namespace ChatApp.Web.Configuration;

public record BlobSettings
{
    public string ConnectionString { get; init; }
    
    public string BlobContainerName { get; init; }
};