using AutoMapper;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IUnitOfWork _unitOfWork;
    private  readonly IMapper _mapper;

    public PaymentMethodService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<PaymentMethodResponse>> GetAllPaymentMethodsAsync(GetAllPaymentMethodsFilter filter)
    {
        try
        {
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
            throw ex;
        }
    }

    public async Task<PaymentMethodResponse> GetPaymentMethodByIdAsync(Guid id)
    {
        try
        {
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetByIdAsync(id);
            if (paymentMethod is null)
            {
                throw new Exception($"Payment Method with {id} not found");
            }
            return  _mapper.Map<PaymentMethodResponse>(paymentMethod);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task CreatePaymentMethodAsync(PaymentMethodRequest paymentMethodRequest)
    {
        try
        {
            var paymentMethod = _mapper.Map<PaymentMethod>(paymentMethodRequest);
            await _unitOfWork.PaymentMethodRepository.CreateAsync(paymentMethod);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdatePaymentMethodAsync(Guid id, PaymentMethodRequest paymentMethodRequest)
    {
        try
        {
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
            throw ex;
        }
    }

    public async Task DeletePaymentMethodAsync(Guid id)
    {
        try
        {
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
            throw ex;
        }
    }
}