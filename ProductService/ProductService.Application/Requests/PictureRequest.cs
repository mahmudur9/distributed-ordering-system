using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Requests;

public class PictureRequest
{
    [Required(ErrorMessage =  "Picture type is required")]
    public int Type { get; set; }
    public IFormFile? MediaFile { get; set; }
    public string? Url { get; set; }
}