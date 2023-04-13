using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service.ServiceBus;

public interface IMessageSerializer
{
    string SerializeMessage(PostMessage message);
    PostMessage DeserializeMessage(string serialized);
}