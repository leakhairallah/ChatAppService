using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service;

public interface IImageService
{
    Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest picture);
    Task<byte[]?> GetProfilePicture(string id);
    Task DeleteProfilePicture(string id);
}