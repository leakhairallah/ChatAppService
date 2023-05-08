using ChatApp.Web.Dtos;
using Newtonsoft.Json;

namespace ChatApp.Web.Service.ServiceBus;

public class JsonMessageSerializer : IMessageSerializer
{
    public string SerializeMessage(PostMessage msg)
    {
        return JsonConvert.SerializeObject(msg);
    }

    public PostMessage DeserializeMessage(string serialized)
    {
        return JsonConvert.DeserializeObject<PostMessage>(serialized);
    }
}