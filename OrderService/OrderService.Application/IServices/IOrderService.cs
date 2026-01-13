using OrderService.Application.Requests;

namespace OrderService.Application.IServices;

public interface IOrderService
{
    Task CreateOrderAsync(OrderRequest orderRequest);
}