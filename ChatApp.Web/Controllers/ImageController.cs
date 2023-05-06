using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Exceptions;
using ChatApp.Web.Service.Images;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        using (_logger.BeginScope("Calling image service..."))
        {
            try
            {
                var response = await _imageService.UpsertProfilePicture(request);
                if (response == null)
                {
                    return StatusCode(502, "Could not upload profile picture.");
                }
            
                var uploadImageResponse = new UploadImageResponse(response.Id);
                return Ok(uploadImageResponse);
            }
            catch (InvalidPictureException e)
            {
                return BadRequest(e.Message);
            }
        }
    }

    
    [HttpGet("{username}")]
    public async Task<IActionResult> DownloadImage(string id)
    {
        using (_logger.BeginScope("Calling image service..."))
        {
            try
            {
                var image = await _imageService.GetProfilePicture(id);
                FileContentResult file = new FileContentResult(image, "image/jpeg");

                return Ok(file);
            }
            catch (Exception e)
            {
                return StatusCode(502, "Could not download image.");
            }
        }
    }
}

