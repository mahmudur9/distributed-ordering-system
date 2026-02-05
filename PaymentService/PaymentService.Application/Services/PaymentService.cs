using AutoMapper;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.ILogging;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAppLogger<PaymentService> _logger;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PaymentResponse>> GetAllPaymentsAsync(GetAllPaymentsFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all payments");
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
            _logger.LogError(ex, "Failed to get all payments");
            throw;
        }
    }

    public async Task<PaymentResponse> GetPaymentByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Getting payment with id {id}");
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(id);
            if (payment is null)
            {
                throw new Exception($"Payment with id {id} not found");
            }
            
            return _mapper.Map<PaymentResponse>(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get payment with id {id}");
            throw;
        }
    }

    public async Task CreatePaymentAsync(PaymentRequest paymentRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new payment");
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
            _logger.LogError(ex, "Failed to create payment");
            throw;
        }
    }

    public async Task UpdatePaymentAsync(Guid id, PaymentRequest paymentRequest)
    {
        try
        {
            _logger.LogInformation($"Updating a payment with id {id}");
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
            _logger.LogError(ex, $"Failed to update payment with id {id}");
            throw;
        }
    }

    public async Task DeletePaymentAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Deleting a payment with id {id}");
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
            _logger.LogError(ex, $"Failed to delete payment with id {id}");
            throw;
        }
    }
}