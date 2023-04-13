namespace ChatApp.Web.Configuration;

public class ServiceBusSettings
{
    public string ConnectionString { get; set; }
    public string SendMessageQueueName { get; set; }
}