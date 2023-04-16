using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service.Profiles;

public interface IProfileService
{
    Task CreateProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task UpdateProfile(Profile profile);
}
