using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service;

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
        var response = new UploadImageResponse(null);
        try
        {
            response = await _imageService.UpsertProfilePicture(request);
        }
        catch (ArgumentException e)
        {
            return UnprocessableEntity("Bad input");
        }
        if (response == null)
        {
            return BadRequest("Could not upload profile picture");
        }
        
        var uploadImageResponse = new UploadImageResponse(response.Id);
        return Ok(uploadImageResponse);
    }

    
    [HttpGet("{username}")]
    public async Task<FileContentResult?> DownloadImage(string username)
    {
        var image = await _imageService.GetProfilePicture(username);
        if (image == null)
        {
            return null;
        }
        FileContentResult file = new FileContentResult(image, "image/jpeg");
        
        return file;
    }
}

