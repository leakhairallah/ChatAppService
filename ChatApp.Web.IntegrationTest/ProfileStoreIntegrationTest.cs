using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;
using ChatApp.Web.Storage.Entities;
using Xunit.Abstractions;

namespace ChatApp.Web.IntegrationTest;

public class ProfileStoreIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IProfileStore _store;

    private readonly Profile _profile = new(
        Username: Guid.NewGuid().ToString(),
        FirstName: "Foo",
        LastName: "Bar",
        ProfilePictureId: "testing"
    );
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteProfile(_profile.Username);
    }

    public ProfileStoreIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IProfileStore>();
    }
    
    [Fact]
    public async Task AddNewProfile()
    {
        await _store.UpsertProfile(_profile);
        Assert.Equal(_profile, await _store.GetProfile(_profile.Username));
    }

}