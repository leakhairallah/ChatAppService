using ChatApp.Web.Storage;
using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service;

public class ProfileService: IProfileService
{
    private readonly IProfileStore _profileStore;
    public ProfileService(IProfileStore profileStore)
    {
        _profileStore = profileStore;
    }
    

    public Task CreateProfile(Profile profile)
    {
        return _profileStore.AddProfile(profile);
    }

    public Task<Profile?> GetProfile(string username)
    {
        return _profileStore.GetProfile(username);
    }

    public Task UpdateProfile(Profile profile)
    {
        return _profileStore.UpsertProfile(profile);
    }
    
}