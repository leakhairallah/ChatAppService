using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;
using ChatApp.Web.Storage.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ChatApp.Web.IntegrationTest
{
    public class ImageStoreIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly IImageStore _store;
        private readonly BlobContainerClient _blobContainerClient;
        private UploadImageRequest _imageId;
        private readonly Profile _profile = new(
            Username: Guid.NewGuid().ToString(),
            FirstName: "Foo",
            LastName: "Bar",
            ProfilePictureId: "a5b22e4c-e1a3-4916-bdf5-857cf757d3d6"
        );
        
        public ImageStoreIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _store = factory.Services.GetRequiredService<IImageStore>();
            _blobContainerClient = factory.Services.GetRequiredService<BlobContainerClient>();
        }

        public async Task InitializeAsync()
        {
            await _blobContainerClient.CreateIfNotExistsAsync();
            var imageBytes = File.ReadAllBytes("IMG_6083.jpeg");
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            var formDataContent = new MultipartFormDataContent
            {
                { imageContent, "File", "IMG_6083.jpeg" }
            };

            var stream = await formDataContent.ReadAsStreamAsync();
            var file = new FormFile(stream, 0, stream.Length, "File", "IMG_6083.jpeg");

            _imageId = new UploadImageRequest(file);
        }

        public async Task DisposeAsync()
        {
            await _store.DeleteProfilePicture(_profile.ProfilePictureId);
            await _blobContainerClient.DeleteIfExistsAsync();
        }

        [Fact]
        public async Task UpsertProfilePicture_ShouldReturnImageId()
        {
            // Act
            var response = await _store.UpsertProfilePicture(_imageId);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Id);
            Assert.Equal(_profile.Username, response.Id);

            // Check that the image was saved to blob storage
            var blobClient = _blobContainerClient.GetBlobClient(response.Id);
            Assert.True(await blobClient.ExistsAsync());
        }

        [Fact]
        public async Task GetProfilePicture_ShouldReturnImageBytes()
        {
            // Act
            var bytes = await _store.GetProfilePicture(_profile.Username);
            
            byte[] imageBytes = File.ReadAllBytes("Image_test.jpeg");

            // Assert
            Assert.NotNull(bytes);
            Assert.Equal(imageBytes, bytes);
        }

        [Fact]
        public async Task GetProfilePicture_ShouldReturnNull_ForNonexistentProfile()
        {
            // Act
            var bytes = await _store.GetProfilePicture("nonexistentuser");

            // Assert
            Assert.Null(bytes);
        }

        [Fact]
        public async Task DeleteProfilePicture_ShouldDeleteBlob()
        {
            // Arrange
            await _store.UpsertProfilePicture(_imageId);

            // Act
            await _store.DeleteProfilePicture(_profile.Username);

            // Assert
            var blobClient = _blobContainerClient.GetBlobClient(_profile.Username);
            Assert.False(await blobClient.ExistsAsync());
        }
    }
}
