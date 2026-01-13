using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Models;

public class ProductOrder : Base
{
    public string ProductName { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal ProductPrice { get; set; }
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }
}