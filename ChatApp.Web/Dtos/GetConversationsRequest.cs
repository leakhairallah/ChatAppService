using System.ComponentModel.DataAnnotations;
using ChatApp.Web.Service.Paginator;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Dtos;

public record GetConversationsRequest(
    [Required] string username, 
    [FromQuery] PaginationFilterConversation filter
    );