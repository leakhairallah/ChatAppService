using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ChatApp.Web.Configuration;
using ChatApp.Web.Dtos;
namespace ChatApp.Web.Service.ServiceBus;

public class SendMessageServiceBusPublisher : ISendMessageServiceBusPublisher
{
    private readonly IMessageSerializer _messageSerializer;
    private readonly ServiceBusSender _sender;

    public SendMessageServiceBusPublisher(
        ServiceBusClient serviceBusClient,
        IMessageSerializer messageSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _messageSerializer = messageSerializer;
        _sender = serviceBusClient.CreateSender(options.Value.SendMessageQueueName);
    }

    public Task Send(PostMessage msg)
    {
        var serialized = _messageSerializer.SerializeMessage(msg);
        return _sender.SendMessageAsync(new ServiceBusMessage(serialized));
    }
}