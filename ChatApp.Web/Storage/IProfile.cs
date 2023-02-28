using ChatApp.Web.Dtos;

namespace ChatApp.Web.Storage;

public interface IProfileStore
{
    Task UpsertProfile(Profile profile);
    Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest picture);
    Task<Profile?> GetProfile(string username);
    Task<byte[]?> GetProfilePicture(string id);
    Task DeleteProfile(string username);
}