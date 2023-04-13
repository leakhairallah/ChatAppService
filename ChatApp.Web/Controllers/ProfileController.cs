using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Profiles;

namespace ChatApp.Web.Controllers;


[ApiController]
[Route("[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        var profile = await _profileService.GetProfile(username);
        if (profile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }

        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        var existingProfile = await _profileService.GetProfile(profile.Username);
        if (existingProfile != null)
        {
            return Conflict($"A user with username {profile.Username} already exists");
        }

        await _profileService.CreateProfile(profile);
        return CreatedAtAction(nameof(GetProfile), new { username = profile.Username },
            profile);
    }
}