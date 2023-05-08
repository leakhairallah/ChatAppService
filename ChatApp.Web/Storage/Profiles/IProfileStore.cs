using ChatApp.Web.Dtos;

namespace ChatApp.Web.Storage.Profiles;

public interface IProfileStore
{
    Task AddProfile(Profile profile);
    Task UpsertProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task DeleteProfile(string username);

}