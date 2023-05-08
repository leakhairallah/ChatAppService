using System.ComponentModel.DataAnnotations;

namespace ChatApp.Web.Dtos;

public record Profile(
    [Required] string Username, 
    [Required] string FirstName, 
    [Required] string LastName,
    string? ProfilePictureId);    