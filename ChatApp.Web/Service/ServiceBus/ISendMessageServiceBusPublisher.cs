using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service.ServiceBus;

public interface ISendMessageServiceBusPublisher
{
    Task Send(string conversationId, SendMessageRequest msg);
}