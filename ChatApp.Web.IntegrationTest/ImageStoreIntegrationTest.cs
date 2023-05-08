using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Images;
using ChatApp.Web.Storage.Profiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Web.IntegrationTest
{
    public class ImageStoreIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly IImageStore _blobStore;
        private readonly IProfileStore _profileStore;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly UploadImageRequest _image;
        private readonly byte[] _byteData = { 0x12, 0x34, 0x56, 0x78, 0x90 };
        private readonly Profile _profile = new(
            Username: Guid.NewGuid().ToString(),
            FirstName: "Foo",
            LastName: "Bar",
            ProfilePictureId: "a5b22e4c-e1a3-4916-bdf5-857cf757d3d6"
        );
        
        public ImageStoreIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _blobStore = factory.Services.GetRequiredService<IImageStore>();
            _blobContainerClient = factory.Services.GetRequiredService<BlobContainerClient>();
            _profileStore = factory.Services.GetRequiredService<IProfileStore>();
            
            MemoryStream ms = new MemoryStream();
            ms.Write(_byteData, 0, _byteData.Length);
            ms.Seek(0, SeekOrigin.Begin);

            string fileName = "RandomFile.bin";
            string contentType = "application/octet-stream";
            long length = ms.Length;
            IFormFile file = new FormFile(ms, 0, length, fileName, contentType);

            _image = new UploadImageRequest(file);
        }

        public async Task InitializeAsync()
        {
            await _blobContainerClient.CreateIfNotExistsAsync();
        }

        public async Task DisposeAsync()
        {
            await _blobStore.DeleteProfilePicture(_profile.ProfilePictureId);
            await _profileStore.DeleteProfile(_profile.Username);
        }

        [Fact]
        public async Task UpsertProfilePicture_ShouldReturnImageId()
        {
            var response = await _blobStore.UpsertProfilePicture(_image);
            
            Assert.NotNull(response);
            Assert.NotNull(response.Id);
            
            var blobClient = _blobContainerClient.GetBlobClient(response.Id);
            Assert.True(await blobClient.ExistsAsync());
        }

        [Fact]
        public async Task GetProfilePicture_ShouldReturnImageBytes()
        {
            var response = await _blobStore.UpsertProfilePicture(_image);

            var updatedProfile = _profile with { ProfilePictureId = response.Id };
            
            await _profileStore.UpsertProfile(updatedProfile);
            
            var bytes = await _blobStore.GetProfilePicture(updatedProfile.Username);

            Assert.NotNull(bytes);
            Assert.Equal(_byteData, bytes);
        }

        [Fact]
        public async Task GetProfilePicture_ShouldReturnNull_ForNonexistentProfile()
        {
            // Act
            var bytes = await _blobStore.GetProfilePicture("nonexistentuser");

            // Assert
            Assert.Null(bytes);
        }

        [Fact]
        public async Task DeleteProfilePicture_ShouldDeleteBlob()
        {
            // Arrange
            await _blobStore.UpsertProfilePicture(_image);

            // Act
            await _blobStore.DeleteProfilePicture(_profile.Username);

            // Assert
            var blobClient = _blobContainerClient.GetBlobClient(_profile.Username);
            Assert.False(await blobClient.ExistsAsync());
        }
    }
}
