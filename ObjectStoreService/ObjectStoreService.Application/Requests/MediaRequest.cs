using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ObjectStoreService.Application.Requests;

public class MediaRequest
{
    [Required(ErrorMessage = "A media file is required")]
    public IFormFile VideoFile { get; set; }
}