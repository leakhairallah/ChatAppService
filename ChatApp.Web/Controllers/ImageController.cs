using Microsoft.AspNetCore.Mvc;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;

namespace ChatApp.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    private readonly IProfileStore _profileStore;

    public ImageController(IProfileStore profileStore)
    {
        _profileStore = profileStore;
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