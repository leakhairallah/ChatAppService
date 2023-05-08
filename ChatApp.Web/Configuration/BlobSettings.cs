namespace ChatApp.Web.Configuration;

public record BlobSettings
{
    public string ConnectionString { get; set; }
    
    public string BlobContainerName { get; set; }
};