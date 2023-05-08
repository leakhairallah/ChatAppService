using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
using ChatApp.Web.Service.Images;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IImageService imageService, ILogger<ImagesController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        _logger.LogInformation("Calling image service for uploading image...");
        try
        {
            var response = await _imageService.UpsertProfilePicture(request);
            return Ok(response);
        }
        catch (InvalidPictureException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(502, e.Message);
        }
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadImage(string id)
    {
        _logger.LogInformation("Calling image service for downloading image...");
        try
        {
            var image = await _imageService.GetProfilePicture(id);
            return Ok(image);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception)
        {
            return StatusCode(502, "Could not download image.");
        }
    }
}

