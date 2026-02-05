using AutoMapper;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.ILogging;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IUnitOfWork _unitOfWork;
    private  readonly IMapper _mapper;
    private readonly IAppLogger<PaymentMethodService> _logger;

    public PaymentMethodService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<PaymentMethodService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PaymentMethodResponse>> GetAllPaymentMethodsAsync(GetAllPaymentMethodsFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all payment methods");
            var paymentMethods = await _unitOfWork.PaymentMethodRepository.GetAllPaymentMethodsAsync(filter.Name, 
                filter.IsActive, filter.ItemsPerPage, filter.PageNumber);
            var paymentTypeCount = await _unitOfWork.PaymentMethodRepository.GetAllPaymentMethodCountAsync(filter.Name, 
                filter.IsActive);

            var paginatedResponse = new PaginatedResponse<PaymentMethodResponse>(
                _mapper.Map<IEnumerable<PaymentMethodResponse>>(paymentMethods),
                paymentTypeCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all payments methods");
            throw;
        }
    }

    public async Task<PaymentMethodResponse> GetPaymentMethodByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Getting payment method with id {id}");
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetByIdAsync(id);
            if (paymentMethod is null)
            {
                throw new Exception($"Payment Method with {id} not found");
            }
            return  _mapper.Map<PaymentMethodResponse>(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get payment method with id {id}");
            throw;
        }
    }

    public async Task CreatePaymentMethodAsync(PaymentMethodRequest paymentMethodRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new payment method");
            var paymentMethod = _mapper.Map<PaymentMethod>(paymentMethodRequest);
            await _unitOfWork.PaymentMethodRepository.CreateAsync(paymentMethod);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment method");
            throw;
        }
    }

    public async Task UpdatePaymentMethodAsync(Guid id, PaymentMethodRequest paymentMethodRequest)
    {
        try
        {
            _logger.LogInformation($"Updating a payment method with id {id}");
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetByIdAsync(id);
            if (paymentMethod is null)
            {
                throw new Exception($"Payment Method with {id} not found");
            }
            
            paymentMethod = _mapper.Map(paymentMethodRequest, paymentMethod);
            paymentMethod.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentMethodRepository.UpdateAsync(paymentMethod);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update payment method with id {id}");
            throw;
        }
    }

    public async Task DeletePaymentMethodAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Deleting a payment method with id {id}");
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetByIdAsync(id);
            if (paymentMethod is null)
            {
                throw new Exception($"Payment Method with {id} not found");
            }
            
            paymentMethod.UpdatedAt = DateTime.UtcNow;
            paymentMethod.IsActive = false;
            await _unitOfWork.PaymentMethodRepository.UpdateAsync(paymentMethod);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete payment method with id {id}");
            throw;
        }
    }
}