namespace ChatApp.Web.Configuration;

public class ServiceBusSettings
{
    public string ConnectionString { get; init; }
    public string SendMessageQueueName { get; init; }
}