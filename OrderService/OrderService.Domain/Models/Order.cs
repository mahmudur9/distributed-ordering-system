namespace OrderService.Domain.Models;

public class Order : Base
{
    public decimal Amount { get; set; }
    public List<ProductOrder> Products { get; set; } = [];
}