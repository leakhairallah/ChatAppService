using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;


namespace ChatApp.Web.Tests.Controllers;

public class ProfileControllerTests: IClassFixture<WebApplicationFactory<Program>>
{
    
    private readonly Mock<IProfileStore> _profileStoreMock = new();
    private readonly HttpClient _httpClient;

    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        // DRY: Don't repeat yourself
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
        }).CreateClient();
    }

    [Fact]
    public async Task AddProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar","1234");
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    

}