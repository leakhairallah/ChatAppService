using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Profiles;

namespace ChatApp.Web.Controllers;


[ApiController]
[Route("api/[controller]")]
public class profileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public profileController(IProfileService profileService)
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
        try
        {
            await _profileService.CreateProfile(profile);
            Console.WriteLine("In Profile Controller");
            Console.WriteLine(profile.ProfilePictureId);
            Console.WriteLine(profile.Username);
            return CreatedAtAction(nameof(GetProfile), new { username = profile.Username },
                profile);
        }
        catch (Exception e){
            
            return Conflict(e.Message);
        }
    }
}