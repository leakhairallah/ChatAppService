using ChatApp.Web.Dtos;

namespace ChatApp.Web.Storage;

public interface IProfileStore
{
    Task UpsertProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task DeleteProfile(string username);

}