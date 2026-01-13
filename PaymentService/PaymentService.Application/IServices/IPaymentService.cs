using PaymentService.Application.Requests;
using PaymentService.Application.Responses;

namespace PaymentService.Application.IServices;

public interface IPaymentService
{
    Task<PaginatedResponse<PaymentResponse>> GetAllPaymentsAsync(GetAllPaymentsFilter filter);
    Task<PaymentResponse> GetPaymentByIdAsync(Guid id);
    Task CreatePaymentAsync(PaymentRequest paymentRequest);
    Task UpdatePaymentAsync(Guid id, PaymentRequest paymentRequest);
    Task DeletePaymentAsync(Guid id);
}