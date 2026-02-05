using OrderService.Application.Requests;
using OrderService.Application.Responses;

namespace OrderService.Application.Abstractions.Gateways;

public interface IProductGateway
{
    Task<GatewayResponse> VerifyAndUpdateProductStockAsync(List<ProductStockGatewayRequest> productStockRequests);
}