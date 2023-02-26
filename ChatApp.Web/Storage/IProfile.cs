using ChatApp.Web.Dtos;

namespace ChatApp.Web.Storage;

public interface IProfileStore
{
    Task UpsertProfile(Profile profile);
    Task UpsertProfilePicture(ProfilePicture picture);
    Task<Profile?> GetProfile(string username);
    Task<byte[]?> GetProfilePicture(string id);
}