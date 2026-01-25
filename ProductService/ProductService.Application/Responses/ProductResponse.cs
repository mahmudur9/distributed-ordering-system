namespace ProductService.Application.Responses;

public class ProductResponse : ResponseBase
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal SellingPrice { get; set; }
    public required int Stock { get; set; }
    public required CategoryResponse Category { get; set; }
    public IEnumerable<PictureResponse> Pictures { get; set; } = [];
}