using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Requests;

public class ProductUpdateRequest
{
    [Required(ErrorMessage =  "Name is required")]
    public string? Name { get; set; }
    [Required(ErrorMessage =  "Description is required")]
    public string? Description { get; set; }
    [Required(ErrorMessage =  "buy price is required")]
    public decimal? BuyPrice { get; set; }
    [Required(ErrorMessage =  "Selling price is required")]
    public decimal? SellingPrice { get; set; }
    [Required(ErrorMessage =  "Stock is required")]
    public int? Stock { get; set; }
    [Required(ErrorMessage =  "CategoryId is required")]
    public Guid? CategoryId { get; set; }
    public List<PictureRequest> Pictures { get; set; } = [];
    public HashSet<Guid> DeletePictureIds { get; set; } = [];
}