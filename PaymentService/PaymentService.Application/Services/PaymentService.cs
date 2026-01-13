using AutoMapper;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<PaymentResponse>> GetAllPaymentsAsync(GetAllPaymentsFilter filter)
    {
        try
        {
            var payments = await _unitOfWork.PaymentRepository.GetAllPaymentsAsync(
                filter.IsActive, filter.DateFrom, filter.DateTo, filter.PaymentId, filter.ItemsPerPage, filter.PageNumber);
            var paymentTypeCount = await _unitOfWork.PaymentRepository.GetAllPaymentCountAsync(
                filter.IsActive, filter.DateFrom, filter.DateTo, filter.PaymentId);

            var paginatedResponse = new PaginatedResponse<PaymentResponse>(
                _mapper.Map<IEnumerable<PaymentResponse>>(payments),
                paymentTypeCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<PaymentResponse> GetPaymentByIdAsync(Guid id)
    {
        try
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(id);
            if (payment is null)
            {
                throw new Exception($"Payment with id {id} not found");
            }
            
            return _mapper.Map<PaymentResponse>(payment);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task CreatePaymentAsync(PaymentRequest paymentRequest)
    {
        try
        {
            if (paymentRequest.PaymentMethodId is null)
            {
                if (await _unitOfWork.PaymentTypeRepository.PaymentTypeHasAnyPaymentMethodAsync(paymentRequest
                        .PaymentTypeId))
                {
                    throw new Exception("Payment type has payment methods");
                }
            }
            else
            {
                if (!await _unitOfWork.PaymentTypeRepository.PaymentTypeMatchesWithPaymentMethodAsync(
                        paymentRequest.PaymentTypeId, (Guid)paymentRequest.PaymentMethodId))
                {
                    throw new Exception("Payment type does not match with payment method");
                }
            }
            
            var payment = _mapper.Map<Payment>(paymentRequest);
            await _unitOfWork.PaymentRepository.CreateAsync(payment);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdatePaymentAsync(Guid id, PaymentRequest paymentRequest)
    {
        try
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(id);
            if (payment is null)
            {
                throw new Exception($"Payment with id {id} not found");
            }
            
            payment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task DeletePaymentAsync(Guid id)
    {
        try
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(id);
            if (payment is null)
            {
                throw new Exception($"Payment with id {id} not found");
            }
            
            payment.UpdatedAt = DateTime.UtcNow;
            payment.IsActive = false;
            await _unitOfWork.PaymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}