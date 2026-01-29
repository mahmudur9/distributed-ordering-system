namespace OrderService.Domain.Models;

public class Order : Base
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public List<ProductOrder> Products { get; set; } = [];
}