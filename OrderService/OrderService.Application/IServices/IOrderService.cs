using OrderService.Application.Requests;
using OrderService.Application.Responses;

namespace OrderService.Application.IServices;

public interface IOrderService
{
    Task CreateOrderAsync(OrderRequest orderRequest);
    Task<PaginatedResponse<OrderResponse>> GetAllOrdersAsync(GetAllOrdersFilter filter);
    Task<PaginatedResponse<OrderResponse>> GetAllOrdersByUserIdAsync(GetAllOrdersFilter filter);
}