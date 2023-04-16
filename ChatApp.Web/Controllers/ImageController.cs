using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Images;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImageController(IImageService imageService)
    {
        _imageService = imageService;
    }
    
    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        try
        {
            var response = await _imageService.UpsertProfilePicture(request);
            if (response == null)
            {
                return BadRequest("Could not upload profile picture");
            }
            
            var uploadImageResponse = new UploadImageResponse(response.Id);
            return Ok(uploadImageResponse);
        }
        catch (ArgumentException e)
        {
            return UnprocessableEntity("Bad input");
        }
    }

    
    [HttpGet("{username}")]
    public async Task<IActionResult> DownloadImage(string username)
    {
        var image = await _imageService.GetProfilePicture(username);
        if (image == null)
        {
            return NotFound("Image not found");
        }
        FileContentResult file = new FileContentResult(image, "image/jpeg");
        
        return Ok(file);
    }
}

