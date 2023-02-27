using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;

namespace ChatApp.Web.Controllers;


[ApiController]
[Route("[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileStore _profileStore;

    public ProfileController(IProfileStore profileStore)
    {
        _profileStore = profileStore;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        var profile = await _profileStore.GetProfile(username);
        if (profile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }

        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        var existingProfile = await _profileStore.GetProfile(profile.Username);
        if (existingProfile != null)
        {
            return Conflict($"A user with username {profile.Username} already exists");
        }

        await _profileStore.UpsertProfile(profile);
        return CreatedAtAction(nameof(GetProfile), new { username = profile.Username },
            profile);
    }

    [HttpPut("{username}")]
    public async Task<ActionResult<Profile>> UpdateProfile(string username, string FirstName, string LastName,
        string imageId)
    {
        var existingProfile = await _profileStore.GetProfile(username);
        if (existingProfile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }

        var profile = new Profile(username, FirstName, LastName, imageId);
        await _profileStore.UpsertProfile(profile);
        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        var response = await _profileStore.UpsertProfilePicture(request);
        return CreatedAtAction(nameof(UploadImage), new { image = request.File.FileName }, response);
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<FileContentResult>> DownloadImage(string id)
    {
        var image = await _profileStore.GetProfilePicture(id);
        if (image == null)
        {
            return NotFound($"No such image found in database");
        }

        return Ok(image);
    }


}