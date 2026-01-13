using PaymentService.Application.Requests;
using PaymentService.Application.Responses;

namespace PaymentService.Application.IServices;

public interface IPaymentMethodService
{
    Task<PaginatedResponse<PaymentMethodResponse>> GetAllPaymentMethodsAsync(GetAllPaymentMethodsFilter  filter);
    Task<PaymentMethodResponse> GetPaymentMethodByIdAsync(Guid id);
    Task CreatePaymentMethodAsync(PaymentMethodRequest paymentMethodRequest);
    Task UpdatePaymentMethodAsync(Guid id, PaymentMethodRequest  paymentMethodRequest);
    Task DeletePaymentMethodAsync(Guid id);
}