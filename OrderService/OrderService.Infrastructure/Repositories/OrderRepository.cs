using OrderService.Domain.IRepositories;
using OrderService.Domain.Models;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(DBContext context) : base(context)
    {
    }
}