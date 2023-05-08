using System.Net;
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

public class ConversationsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IConversationsService> _conversationsServiceMock = new();
    private readonly HttpClient _httpClient;

    public ConversationsControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => {
                services.AddSingleton(_conversationsServiceMock.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task AddConversation()
    {
        var conversation = new StartConversation(new[] { "foo", "bar" }, new SendMessageRequest("", "foo", "hello!"));
        var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);

        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ReturnsAsync(startConversationResponse);
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

        _testOutputHelper.WriteLine(response.ToString());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Once);
    }
    
    [Fact]
public async Task GetConversations()
{
    var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);

    _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
        .ReturnsAsync(startConversationResponse);

    var conv1 = new StartConversation(new[] { "foo", "barr" }, new SendMessageRequest("", "foo", "hello!"));
    await _httpClient.PostAsync("api/Conversations",
        new StringContent(JsonConvert.SerializeObject(conv1), Encoding.Default, "application/json"));

    var conv2 = new StartConversation(new[] { "foo", "user1" }, new SendMessageRequest("", "foo", "hello user 1!"));
    await _httpClient.PostAsync("api/Conversations",
        new StringContent(JsonConvert.SerializeObject(conv2), Encoding.Default, "application/json"));

    var conv3 = new StartConversation(new[] { "user2", "foo" }, new SendMessageRequest("", "foo", "hello user 2!"));
    await _httpClient.PostAsync("api/Conversations",
        new StringContent(JsonConvert.SerializeObject(conv3), Encoding.Default, "application/json"));

    var conv4 = new StartConversation(new[] { "user3", "foo" }, new SendMessageRequest("", "user3", "hello foo!"));
    await _httpClient.PostAsync("api/Conversations",
        new StringContent(JsonConvert.SerializeObject(conv4), Encoding.Default, "application/json"));

    var conv5 = new StartConversation(new[] { "foo", "user4" }, new SendMessageRequest("", "foo", "hello user 4!"));
    await _httpClient.PostAsync("api/Conversations",
        new StringContent(JsonConvert.SerializeObject(conv5), Encoding.Default, "application/json"));

    var conv6 = new StartConversation(new[] { "user5", "foo" }, new SendMessageRequest("", "user5", "hello foo!"));
    await _httpClient.PostAsync("api/Conversations",
        new StringContent(JsonConvert.SerializeObject(conv6), Encoding.Default, "application/json"));

    _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Exactly(6));

    var getUserConversationsResponse = new GetUserConversationsResponse(null, null);
    var filter = new PaginationFilterConversation(6, "", 0);

    _conversationsServiceMock.Setup(m => m.GetUserConversations(It.IsAny<string>(), It.IsAny<PaginationFilterConversation>(), It.IsAny<HttpRequest>()))
        .ReturnsAsync(getUserConversationsResponse);

    var filter2 = new PaginationFilterConversation(6, "", 0);
    var user = "foo";

    // var response = await _httpClient.GetAsync($"api/Conversations/foo/{filter2}");

    // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

    [Theory]
    [InlineData(null, "Foo")]
    [InlineData(" ", "Foo")]
    [InlineData("Foo", " ")]
    [InlineData("Foo", null)]
    public async Task AddConversation_NullOrWhiteSpace(string user1, string user2)
    {
        var conversation = new StartConversation(new[] { user1, user2}, new SendMessageRequest("", user1, "hello!"));
        var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);

        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ThrowsAsync(new InvalidConversationException("message", HttpStatusCode.NotFound));
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

        _testOutputHelper.WriteLine(response.ToString());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Theory]
    [InlineData("Ivy", "Foo")]
    [InlineData("Foo", "Ivy")]
    public async Task AddConversation_NotFoundException(string user1, string user2)
    {
        var conversation = new StartConversation(new[] { user1, user2}, new SendMessageRequest("", user1, "hello!"));
        var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);

        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ThrowsAsync(new NotFoundException("message", "", HttpStatusCode.NotFound));
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

        _testOutputHelper.WriteLine(response.ToString());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Once);   
    }
    
    [Fact]
    public async Task AddConversation_tooMany()
    {
        var conversation = new StartConversation(new[] { "user1", "user2", "user3"}, new SendMessageRequest("", "user1", "hello!"));
        var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);

        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ThrowsAsync(new InvalidConversationException("message", HttpStatusCode.NotFound));
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

        _testOutputHelper.WriteLine(response.ToString());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Once);   
    }
    
    [Fact]
    public async Task AddConversation_Conflict()
    {
        var conversation = new StartConversation(new[] { "user1", "user2"}, new SendMessageRequest("", "user1", "hello!"));
        var startConversationResponse = new StartConversationResponse(Guid.NewGuid().ToString(), long.MinValue);

        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ReturnsAsync(startConversationResponse);
        
        var response1 = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

        _testOutputHelper.WriteLine(response1.ToString());
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Once);
        
        _conversationsServiceMock.Setup(m => m.AddConversation(It.IsAny<StartConversation>()))
            .ThrowsAsync(new ConflictException("message","item", HttpStatusCode.NotFound));
        
        var response2 = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

        _testOutputHelper.WriteLine(response2.ToString());
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Exactly(2));
        
        var conversation2 = new StartConversation(new[] { "user2", "user1"}, new SendMessageRequest("", "user2", "hello!"));

        var response3 = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation2), Encoding.Default, "application/json"));

        _testOutputHelper.WriteLine(response3.ToString());
        Assert.Equal(HttpStatusCode.Conflict, response3.StatusCode);
        _conversationsServiceMock.Verify(mock => mock.AddConversation(It.IsAny<StartConversation>()), Times.Exactly(3));
    }
}