using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatApp.Web.Dtos;
using System.Text;
using ChatApp.Web.Service.Conversations;
using ChatApp.Web.Service.Messages;
using ChatApp.Web.Service.Profiles;
using Xunit.Abstractions;


namespace ChatApp.Web.Tests.Controllers;

public class ConversationsControllerTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IConversationsService> _conversationServiceMock = new();
    private readonly Mock<IProfileService> _profileServiceMock = new();
    private readonly Mock<IMessageService> _messageServiceMock = new();
    private readonly HttpClient _httpClient;

    public ConversationsControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { 
                services.AddSingleton(_conversationServiceMock.Object);
                services.AddSingleton(_profileServiceMock.Object);
                services.AddSingleton(_messageServiceMock.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task AddConversation()
    {
        var foo = new Profile("foo", "Foo", "Bar","1234");
        var bar = new Profile("bar", "Bar", "Foo", "1234");
        await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(foo), Encoding.Default, "application/json"));

        var resp = await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(bar), Encoding.Default, "application/json"));
        
        var conversation = new StartConversation( new SendMessageRequest("id", "Hello!", "foo"),new [] {"foo", "bar"});
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));
        _testOutputHelper.WriteLine(response.ToString());
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