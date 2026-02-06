using AutoMapper;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.ILogging;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.Application.Services;

public class PaymentTypeService : IPaymentTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAppLogger<PaymentTypeService> _logger;

    public PaymentTypeService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<PaymentTypeService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PaymentTypeResponse>> GetAllPaymentTypesAsync(GetAllPaymentTypesFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all payment types");
            var paymentTypes = await _unitOfWork.PaymentTypeRepository.GetAllPaymentTypesAsync(filter.Name, 
                filter.IsActive, filter.ItemsPerPage, filter.PageNumber);
            var paymentTypeCount = await _unitOfWork.PaymentTypeRepository.GetAllPaymentTypeCountAsync(filter.Name, 
                filter.IsActive);

            var paginatedResponse = new PaginatedResponse<PaymentTypeResponse>(
                _mapper.Map<IEnumerable<PaymentTypeResponse>>(paymentTypes),
                paymentTypeCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all payments types");
            throw;
        }
    }

    public async Task<PaymentTypeResponse> GetPaymentTypeByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Getting payment type with id {id}");
            var paymentType = await _unitOfWork.PaymentTypeRepository.GetByIdAsync(id);
            if (paymentType is null)
            {
                throw new KeyNotFoundException($"Payment Type with {id} not found");
            }
            return  _mapper.Map<PaymentTypeResponse>(paymentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get payment type with id {id}");
            throw;
        }
    }

    public async Task CreatePaymentTypeAsync(PaymentTypeRequest paymentTypeRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new payment type");
            var paymentType = _mapper.Map<PaymentType>(paymentTypeRequest);
            await _unitOfWork.PaymentTypeRepository.CreateAsync(paymentType);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment type");
            throw;
        }
    }

    public async Task UpdatePaymentTypeAsync(Guid id, PaymentTypeRequest paymentTypeRequest)
    {
        try
        {
            _logger.LogInformation($"Updating a payment type with id {id}");
            var paymentType = await _unitOfWork.PaymentTypeRepository.GetByIdAsync(id);
            if (paymentType is null)
            {
                throw new KeyNotFoundException($"Payment Type with {id} not found");
            }
            
            paymentType = _mapper.Map(paymentTypeRequest, paymentType);
            paymentType.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentTypeRepository.UpdateAsync(paymentType);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update payment type with id {id}");
            throw;
        }
    }

    public async Task DeletePaymentTypeAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Deleting a payment type with id {id}");
            var paymentType = await _unitOfWork.PaymentTypeRepository.GetByIdAsync(id);
            if (paymentType is null)
            {
                throw new KeyNotFoundException($"Payment Type with {id} not found");
            }
            
            paymentType.IsActive = false;
            paymentType.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentTypeRepository.UpdateAsync(paymentType);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete payment type with id {id}");
            throw;
        }
    }
}