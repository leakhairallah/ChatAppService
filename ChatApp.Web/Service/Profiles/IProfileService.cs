﻿using ChatApp.Web.Dtos;

namespace ChatApp.Web.Service.Profiles;

public interface IProfileService
{
    // Task EnqueueCreateProfile(Profile profile);
    Task CreateProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task UpdateProfile(Profile profile);
}