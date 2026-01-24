using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Requests;

public class ProductRequest
{
    [Required(ErrorMessage =  "Name is required")]
    public string Name { get; set; }
    [Required(ErrorMessage =  "Description is required")]
    public required string Description { get; set; }
    [Required(ErrorMessage =  "buy price is required")]
    public required decimal BuyPrice { get; set; }
    [Required(ErrorMessage =  "Selling price is required")]
    public required decimal SellingPrice { get; set; }
    [Required(ErrorMessage =  "Stock is required")]
    public required int Stock { get; set; }
    [Required(ErrorMessage =  "CategoryId is required")]
    public Guid CategoryId { get; set; }
    public List<PictureRequest> Pictures { get; set; } = [];
}