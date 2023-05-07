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
using ChatApp.Web.Service.Paginator;
using ChatApp.Web.Service.Profiles;
using Xunit.Abstractions;

namespace ChatApp.Web.Tests.Controllers;

public class ConversationsControllerTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IConversationsService> _conversationsServiceMock = new();
    private readonly Mock<IProfileService> _profileServiceMock = new();
    private readonly Mock<IMessageService> _messageServiceMock = new();
    private readonly HttpClient _httpClient;

    public ConversationsControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { 
                services.AddSingleton(_conversationsServiceMock.Object);
                services.AddSingleton(_profileServiceMock.Object);
                services.AddSingleton(_messageServiceMock.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task AddConversation()
    {
        var foo = new Profile("fooo", "Foo", "Bar","1234");
        var bar = new Profile("barr", "Bar", "Foo", "1234");
        await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(foo), Encoding.Default, "application/json"));

        var resp = await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(bar), Encoding.Default, "application/json"));
        
        var conversation = new StartConversation( new [] {"fooo", "barr"}, new SendMessageRequest("", "fooo", "hello!"));
        
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));
        _testOutputHelper.WriteLine(response.ToString());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    

    // [Fact]
    // public async Task GetConversations()
    // {
    //     var foo = new Profile("fooo", "Foo", "Bar","1234");
    //     var bar = new Profile("barr", "Bar", "Foo", "1234");
    //     var user1 = new Profile("user1", "user 1", "name", "27819");
    //     var user2 = new Profile("user2", "user 2", "name", "27819");
    //     var user3 = new Profile("user3", "user 3", "name", "27819");
    //     var user4 = new Profile("user4", "user 4", "name", "27819");
    //     var user5 = new Profile("user5", "user 5", "name", "27819");
    //
    //     await _httpClient.PostAsync("api/Profile",
    //         new StringContent(JsonConvert.SerializeObject(foo), Encoding.Default, "application/json"));
    //
    //     await _httpClient.PostAsync("api/Profile",
    //         new StringContent(JsonConvert.SerializeObject(bar), Encoding.Default, "application/json"));
    //     
    //     await _httpClient.PostAsync("api/Profile",
    //         new StringContent(JsonConvert.SerializeObject(user1), Encoding.Default, "application/json"));
    //     
    //     await _httpClient.PostAsync("api/Profile",
    //         new StringContent(JsonConvert.SerializeObject(user2), Encoding.Default, "application/json"));
    //     
    //     await _httpClient.PostAsync("api/Profile",
    //         new StringContent(JsonConvert.SerializeObject(user3), Encoding.Default, "application/json"));
    //     
    //     await _httpClient.PostAsync("api/Profile",
    //         new StringContent(JsonConvert.SerializeObject(user4), Encoding.Default, "application/json"));
    //
    //     await _httpClient.PostAsync("api/Profile",
    //         new StringContent(JsonConvert.SerializeObject(user5), Encoding.Default, "application/json"));
    //
    //     var conv1 = new StartConversation( new [] {"fooo", "barr"}, new SendMessageRequest("", "fooo", "hello!"));
    //     await _httpClient.PostAsync("api/Conversations",
    //         new StringContent(JsonConvert.SerializeObject(conv1), Encoding.Default, "application/json"));
    //     
    //     var conv2 = new StartConversation( new [] {"fooo", "user1"}, new SendMessageRequest("", "fooo", "hello user 1!"));
    //     await _httpClient.PostAsync("api/Conversations",
    //         new StringContent(JsonConvert.SerializeObject(conv2), Encoding.Default, "application/json"));
    //     
    //     var conv3 = new StartConversation( new [] {"user2", "fooo"}, new SendMessageRequest("", "fooo", "hello user 2!"));
    //     await _httpClient.PostAsync("api/Conversations",
    //         new StringContent(JsonConvert.SerializeObject(conv3), Encoding.Default, "application/json"));
    //     
    //     var conv4 = new StartConversation( new [] {"user3", "fooo"}, new SendMessageRequest("", "user3", "hello fooo!"));
    //     await _httpClient.PostAsync("api/Conversations",
    //         new StringContent(JsonConvert.SerializeObject(conv4), Encoding.Default, "application/json"));
    //     
    //     var conv5 = new StartConversation( new [] {"fooo", "user4"}, new SendMessageRequest("", "fooo", "hello user 4!"));
    //     await _httpClient.PostAsync("api/Conversations",
    //         new StringContent(JsonConvert.SerializeObject(conv5), Encoding.Default, "application/json"));
    //     
    //     var conv6 = new StartConversation( new [] {"user5", "fooo"}, new SendMessageRequest("", "user5", "hello fooo!"));
    //     await _httpClient.PostAsync("api/Conversations",
    //         new StringContent(JsonConvert.SerializeObject(conv6), Encoding.Default, "application/json"));
    //
    //     var filter = new GetConversationsRequest("fooo",new PaginationFilterConversation(6, "", 0));
    //     
    //     var response = await _httpClient.GetAsync($"api/Conversations/{filter}");
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    // }

    [Fact]
    public async Task AddConversation_UserNotFound()
    {
        var foo = new Profile("fooo", "Foo", "Bar","1234");
        await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(foo), Encoding.Default, "application/json"));
        var conversation = new StartConversation(new [] {"", "", "user", "karima"}, new SendMessageRequest("", "fooo", "hello"));
        var response = await _httpClient.PostAsync("api/Conversations",
            new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest , response.StatusCode);   
    }

}