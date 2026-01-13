using PaymentService.Application.Requests;
using PaymentService.Application.Responses;

namespace PaymentService.Application.IServices;

public interface IPaymentTypeService
{
    Task<PaginatedResponse<PaymentTypeResponse>> GetAllPaymentTypesAsync(GetAllPaymentTypesFilter  filter);
    Task<PaymentTypeResponse> GetPaymentTypeByIdAsync(Guid id);
    Task CreatePaymentTypeAsync(PaymentTypeRequest paymentTypeRequest);
    Task UpdatePaymentTypeAsync(Guid id, PaymentTypeRequest paymentTypeRequest);
    Task DeletePaymentTypeAsync(Guid id);
}