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
        // Arrange
        var profile = _profile;

        // Act
        await _store.UpsertProfile(profile);
        var result = await _store.GetProfile(profile.Username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(profile.Username, result!.Username);
        Assert.Equal(profile.FirstName, result.FirstName);
        Assert.Equal(profile.LastName, result.LastName);
        Assert.Equal(profile.ProfilePictureId, result.ProfilePictureId);
    }

    [Fact]
    public async Task UpdateProfile()
    {
        // Arrange
        var profile = _profile;
        await _store.UpsertProfile(profile);
        var newProfile = new Profile(
            Username: profile.Username,
            FirstName: "Baz",
            LastName: "Qux",
            ProfilePictureId: "updated"
        );

        // Act
        await _store.UpsertProfile(newProfile);
        var result = await _store.GetProfile(profile.Username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newProfile.Username, result!.Username);
        Assert.Equal(newProfile.FirstName, result.FirstName);
        Assert.Equal(newProfile.LastName, result.LastName);
        Assert.Equal(newProfile.ProfilePictureId, result.ProfilePictureId);
    }

    [Fact]
    public async Task DeleteProfile()
    {
        // Arrange
        var profile = _profile;
        await _store.UpsertProfile(profile);

        // Act
        await _store.DeleteProfile(profile.Username);
        var result = await _store.GetProfile(profile.Username);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertProfile_NullProfile_ThrowsArgumentException()
    {
        // Arrange
        Profile? profile = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _store.UpsertProfile(profile!));
    }

    [Fact]
    public async Task UpsertProfile_NullOrWhiteSpaceUsername_ThrowsArgumentException()
    {
        // Arrange
        var profile = new Profile(
            Username: null!,
            FirstName: "FirstName",
            LastName: "LastName",
            ProfilePictureId: "ProfilePictureId"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _store.UpsertProfile(profile));

        profile = new Profile(
            Username: "",
            FirstName: "FirstName",
            LastName: "LastName",
            ProfilePictureId: "ProfilePictureId"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _store.UpsertProfile(profile));

        profile = new Profile(
            Username: "   ",
            FirstName: "FirstName",
            LastName: "LastName",
            ProfilePictureId: "ProfilePictureId"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _store.UpsertProfile(profile));
    }

}