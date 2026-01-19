using Grpc.Core;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;

namespace PaymentService.API.Grpc;

public class PaymentGrpc : PaymentGrpcService.PaymentGrpcServiceBase
{
    private readonly IPaymentService _paymentService;

    public PaymentGrpc(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public override async Task<CreatePaymentGrpcResponse> CreatePayment(CreatePaymentGrpcRequest request,
        ServerCallContext context)
    {
        var response = new CreatePaymentGrpcResponse();
        try
        {
            var payment = new PaymentRequest()
            {
                Amount = decimal.Parse(request.Amount),
                OrderId = Guid.Parse(request.OrderId),
                PaymentTypeId = Guid.Parse(request.PaymentTypeId),
                PaymentMethodId = !string.IsNullOrEmpty(request.PaymentMethodId) ? Guid.Parse(request.PaymentMethodId) : null
            };
            await _paymentService.CreatePaymentAsync(payment);

            response.Success = true;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Error = ex.Message;
            return response;
        }
    }
}