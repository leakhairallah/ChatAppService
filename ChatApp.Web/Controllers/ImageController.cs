using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageStore _profileStore;

    public ImageController(IImageStore profileStore)
    {
        _profileStore = profileStore;
    }


    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        var response = await _profileStore.UpsertProfilePicture(request);
        return CreatedAtAction(nameof(DownloadImage), new { image = response.Id }, response);
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