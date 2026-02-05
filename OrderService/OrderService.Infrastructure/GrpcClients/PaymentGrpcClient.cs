using System.Globalization;
using OrderService.Application.Abstractions.Gateways;
using OrderService.Application.Requests;
using OrderService.Application.Responses;
using PaymentService.API;

namespace OrderService.Infrastructure.GrpcClients;

public class PaymentGrpcClient : IPaymentGateway
{
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcServiceClient;

    public PaymentGrpcClient(PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcServiceClient)
    {
        _paymentGrpcServiceClient = paymentGrpcServiceClient;
    }

    public async Task<GatewayResponse> CreatePaymentAsync(CreatePaymentGatewayRequest request)
    {
        var payment = new CreatePaymentGrpcRequest();
        payment.OrderId = request.OrderId.ToString();
        payment.Amount = request.Amount.ToString(CultureInfo.InvariantCulture);
        payment.PaymentTypeId = request.PaymentTypeId.ToString();
        payment.PaymentMethodId = request.PaymentMethodId.ToString();
        var paymentResponse = await _paymentGrpcServiceClient.CreatePaymentAsync(payment);

        return new GatewayResponse()
        {
            Success = paymentResponse.Success,
            Error =  paymentResponse.Error
        };
    }
}