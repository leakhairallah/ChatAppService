using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatApp.Web.Dtos;
using System.Text;
using ChatApp.Web.Exceptions;
using ChatApp.Web.Service.Conversations;
using ChatApp.Web.Service.Messages;
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Service.Profiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace ChatApp.Web.Tests.Controllers;

public class MessageControllerTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IMessageService> _messagesServiceMock = new();
    private readonly Mock<IConversationsService> _conversationsServiceMock = new();
    private readonly HttpClient _httpClient;

    public MessageControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_messagesServiceMock.Object);
                services.AddSingleton(_conversationsServiceMock.Object);
            });
        }).CreateClient();
    }
    
    [Fact]
    public async Task SendMessageTest()
    {
        var conversation = new StartConversation(new[] { "foo", "bar" }, new SendMessageRequest("", "foo", "hello!"));
        var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);
        
        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ReturnsAsync(startConversationResponse);
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Once);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var deserializedResponse = JsonConvert.DeserializeObject<StartConversationResponse>(responseContent);
        var id = deserializedResponse.Id;

        var sendMessageRequest = new SendMessageRequest(id, "foo", "Hello!");
        _messagesServiceMock
            .Setup(m => m.PostMessageToConversation(It.IsAny<string>(), It.IsAny<SendMessageRequest>(), It.IsAny<long>()))
            .ReturnsAsync(new UploadMessageResponse(0));
        var response2 = await _httpClient.PostAsync($"api/conversations/{id}/messages",
            new StringContent(JsonConvert.SerializeObject(sendMessageRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    }

    [Fact]
    public async Task SendMessageTest_fail()
    {
        var conversation = new StartConversation(new[] { "foo", "bar" }, new SendMessageRequest("", "foo", "hello!"));
        var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);
        
        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ReturnsAsync(startConversationResponse);
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Once);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var deserializedResponse = JsonConvert.DeserializeObject<StartConversationResponse>(responseContent);
        var id = deserializedResponse.Id;

        var sendMessageRequest = new SendMessageRequest(id, "foo", "Hello!");
        _messagesServiceMock
            .Setup(m => m.PostMessageToConversation(It.IsAny<string>(), It.IsAny<SendMessageRequest>(), It.IsAny<long>()))
            .ReturnsAsync(new UploadMessageResponse(0));
        var response2 = await _httpClient.PostAsync($"api/conversations/{id}/messages",
            new StringContent(JsonConvert.SerializeObject(sendMessageRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    }
}