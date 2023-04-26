using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Conversations;
using ChatApp.Web.Storage.Profiles;

namespace ChatApp.Web.Tests.Controllers;

public class ConversationControllerTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IConversationStore> _conversationStoreMock = new();
    private readonly Mock<IConversationParticipantsStore> _conversationParticipantsStoreMock = new();
    private readonly HttpClient _httpClient;

    public ConversationControllerTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { 
                services.AddSingleton(_conversationStoreMock.Object);
                services.AddSingleton(_conversationParticipantsStoreMock.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task AddConversation()
    {
        var conversation = new StartConversation(new [] {"foo", "bar"}, new PostMessage("id", "Hello!", "foo"));
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // [Fact]
    // public async Task AddConversation_UserNotFound()
    // {
    //     var conversation = new StartConversation(new [] {"foo", "bar"}, new PostMessage("id", "Hello!", "foo"));
    //     var response = await _httpClient.PostAsync("api/Conversations",
    //         new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);   
    // }

}