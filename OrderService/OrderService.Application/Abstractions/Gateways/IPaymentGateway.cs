using OrderService.Application.Requests;
using OrderService.Application.Responses;

namespace OrderService.Application.Abstractions.Gateways;

public interface IPaymentGateway
{
    Task<GatewayResponse> CreatePaymentAsync(CreatePaymentGatewayRequest request);
}