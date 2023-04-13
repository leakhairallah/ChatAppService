using ChatApp.Web.Dtos;
using ChatApp.Web.Storage.Images;

namespace ChatApp.Web.Service.Images;

public class ImageService : IImageService
{
    private readonly IImageStore _imageStore;
    public ImageService(IImageStore imageStore)
    {
        _imageStore = imageStore;
    }
    public Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest picture)
    {
        return _imageStore.UpsertProfilePicture(picture);
    }

    public Task<byte[]?> GetProfilePicture(string id)
    {
        return _imageStore.GetProfilePicture(id);
    }

    public Task DeleteProfilePicture(string id)
    {
        return _imageStore.DeleteProfilePicture(id);
    }
}