using System.ComponentModel.DataAnnotations;

namespace ObjectStoreService.Application.Requests;

public class MediaDeleteRequest
{
    [Required(ErrorMessage = "Url is required")]
    public string Url { get; set; }
}