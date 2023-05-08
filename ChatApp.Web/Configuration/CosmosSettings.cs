using Azure.Security.KeyVault.Secrets;

namespace ChatApp.Web.Configuration;

public record CosmosSettings
{
    public string ConnectionString { get; set; }
}