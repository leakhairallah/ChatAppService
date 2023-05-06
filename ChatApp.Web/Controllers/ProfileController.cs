using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
using ChatApp.Web.Service.Profiles;

namespace ChatApp.Web.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IProfileService profileService, ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        using (_logger.BeginScope("Calling profile service..."))
        {
            try
            {
                var profile = await _profileService.GetProfile(username);
                if (profile == null)
                {
                    return NotFound($"A User with username {username} was not found");
                }

                return Ok(profile);
            }
            catch (Exception e)
            {
                return StatusCode(502, e.Message);
            }
        }
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        using (_logger.BeginScope("Calling profile service..."))
        {
            try
            {
                await _profileService.CreateProfile(profile);
                return CreatedAtAction(nameof(GetProfile), new { username = profile.Username },
                    profile);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (InvalidProfileException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e){
            
                return StatusCode(502, e.Message);
            }
        }
    }
}