using ChatApp.Web.Dtos;

namespace ChatApp.Web.Storage;

public interface IImageStore
{
    Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest picture);
    Task<byte[]?> GetProfilePicture(string id);
}