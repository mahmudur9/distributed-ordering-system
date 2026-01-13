using AutoMapper;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.Application.Services;

public class PaymentTypeService : IPaymentTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<PaymentTypeResponse>> GetAllPaymentTypesAsync(GetAllPaymentTypesFilter filter)
    {
        try
        {
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
            throw ex;
        }
    }

    public async Task<PaymentTypeResponse> GetPaymentTypeByIdAsync(Guid id)
    {
        try
        {
            var paymentType = await _unitOfWork.PaymentTypeRepository.GetByIdAsync(id);
            if (paymentType is null)
            {
                throw new Exception($"Payment Type with {id} not found");
            }
            return  _mapper.Map<PaymentTypeResponse>(paymentType);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task CreatePaymentTypeAsync(PaymentTypeRequest paymentTypeRequest)
    {
        try
        {
            var paymentType = _mapper.Map<PaymentType>(paymentTypeRequest);
            await _unitOfWork.PaymentTypeRepository.CreateAsync(paymentType);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdatePaymentTypeAsync(Guid id, PaymentTypeRequest paymentTypeRequest)
    {
        try
        {
            var paymentType = await _unitOfWork.PaymentTypeRepository.GetByIdAsync(id);
            if (paymentType is null)
            {
                throw new Exception($"Payment Type with {id} not found");
            }
            
            paymentType = _mapper.Map(paymentTypeRequest, paymentType);
            paymentType.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentTypeRepository.UpdateAsync(paymentType);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task DeletePaymentTypeAsync(Guid id)
    {
        try
        {
            var paymentType = await _unitOfWork.PaymentTypeRepository.GetByIdAsync(id);
            if (paymentType is null)
            {
                throw new Exception($"Payment Type with {id} not found");
            }
            
            paymentType.IsActive = false;
            paymentType.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentTypeRepository.UpdateAsync(paymentType);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}