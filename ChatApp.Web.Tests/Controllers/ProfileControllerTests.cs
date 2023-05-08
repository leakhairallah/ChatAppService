using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
using ChatApp.Web.Service.Profiles;

namespace ChatApp.Web.Tests.Controllers;

public class ProfileControllerTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileService> _profileServiceMock = new();
    private readonly HttpClient _httpClient;
    
    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileServiceMock.Object); });
        }).CreateClient();
    }

    [Fact]
    public async Task AddProfile()
    {
        
        var profile = new Profile("foobar", "Foo", "Bar","1234");
        var response = await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task GetProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar","1234");
        _profileServiceMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);

        var response = await _httpClient.GetAsync($"api/Profile/{profile.Username}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(json));
    }
    
    [Fact]
    public async Task GetProfile_NotFound()
    {
        _profileServiceMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);

        var response = await _httpClient.GetAsync($"api/Profile/foobar");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddProfile_Conflict()
    {
        var profile = new Profile("foobar", "Foo", "Bar", "1234");
        _profileServiceMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);

        var response = await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        _profileServiceMock.Setup(m => m.GetProfile(profile.Username))
            .ThrowsAsync(new ConflictException("","", HttpStatusCode.BadRequest));

        var response2 = await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        // Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        _profileServiceMock.Verify(m => m.CreateProfile(profile), Times.Exactly(2));
    }
    
    [Theory]
    [InlineData(null, "Foo", "Bar","1234")]
    [InlineData("", "Foo", "Bar","1234")]
    [InlineData(" ", "Foo", "Bar","1234")]
    [InlineData("foobar", null, "Bar","1234")]
    [InlineData("foobar", "", "Bar","1234")]
    [InlineData("foobar", "   ", "Bar","1234")]
    [InlineData("foobar", "Foo", "","1234")]
    [InlineData("foobar", "Foo", null,"1234")]
    [InlineData("foobar", "Foo", " ","1234")]
    
    public async Task AddProfile_InvalidArgs(string username, string firstName, string lastName, string imageID)
    {
        var profile = new Profile(username, firstName, lastName, imageID);
        var response = await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileServiceMock.Verify(mock => mock.CreateProfile(profile), Times.Never);
    }
}