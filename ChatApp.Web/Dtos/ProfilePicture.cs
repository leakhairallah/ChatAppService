using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record ProfilePicture(
    [Required] IFormFile File);