using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Requests;

public class PictureRequest
{
    [Required(ErrorMessage = "A valid image is required")]
    public IFormFile MediaFile { get; set; }
}