using AutoMapper;
using OrderService.Application.IServices;
using OrderService.Application.Requests;
using OrderService.Domain.IRepositories;
using OrderService.Domain.Models;
using PaymentService.API;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcServiceClient;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcServiceClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentGrpcServiceClient = paymentGrpcServiceClient;
    }

    public async Task CreateOrderAsync(OrderRequest orderRequest)
    {
        try
        {
            var order = _mapper.Map<Order>(orderRequest);
            order.Amount = orderRequest.Payment.Amount;
            
            await _unitOfWork.BeginTransactionAsync();
            
            await _unitOfWork.OrderRepository.CreateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var payment = _mapper.Map<CreatePaymentGrpcRequest>(orderRequest.Payment);
            payment.OrderId = order.Id.ToString();
            var response = await _paymentGrpcServiceClient.CreatePaymentAsync(payment);
            if (response.Success)
            {
                await _unitOfWork.CommitTransactionAsync();
                return;
            }
            
            throw new Exception("Payment Failed");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw ex;
        }
    }
}