using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;
using ChatApp.Web.Storage.Entities;
using Xunit.Abstractions;

namespace ChatApp.Web.IntegrationTest;

public class ImageStoreIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IImageStore _store;

    private readonly UploadImageRequest _profile = new(
        
    );
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteProfilePicture(_profile.Username);
    }

    public ImageStoreIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IProfileStore>();
    }
    
    [Fact]
    public async Task AddNewProfile()
    {
        await _store.UpsertProfilePicture(_profile);
        Assert.Equal(_profile, await _store.GetProfile(_profile.Username));
    }

}