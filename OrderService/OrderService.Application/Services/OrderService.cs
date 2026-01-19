using AutoMapper;
using OrderService.Application.IServices;
using OrderService.Application.Requests;
using OrderService.Domain.IRepositories;
using OrderService.Domain.Models;
using PaymentService.API;
using ProductService.API;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcServiceClient;
    private readonly ProductGrpcService.ProductGrpcServiceClient _productGrpcServiceClient;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcServiceClient, ProductGrpcService.ProductGrpcServiceClient productGrpcServiceClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentGrpcServiceClient = paymentGrpcServiceClient;
        _productGrpcServiceClient = productGrpcServiceClient;
    }

    private UpdateProductGrpcStockRequest MapProductStock(OrderRequest orderRequest)
    {
        var productStock = new UpdateProductGrpcStockRequest();
        foreach (var product in orderRequest.Products)
        {
            productStock.Products.Add(new ProductStockGrpcRequest()
            {
                Id = product.ProductId.ToString(),
                Quantity = product.Quantity,
                Price = product.ProductPrice.ToString()
            });
        }
        
        return productStock;
    }

    public async Task CreateOrderAsync(OrderRequest orderRequest)
    {
        try
        {
            var order = _mapper.Map<Order>(orderRequest);
            decimal amount = 0;
            foreach (var product in orderRequest.Products)
            {
                amount += product.ProductPrice * product.Quantity;
            }
            order.Amount = amount;
            
            await _unitOfWork.BeginTransactionAsync();
            
            await _unitOfWork.OrderRepository.CreateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var productStock = MapProductStock(orderRequest);
            var productStockResponse = await _productGrpcServiceClient.VerifyAndUpdateProductStockAsync(productStock);
            if (!productStockResponse.Success)
            {
                throw new Exception(productStockResponse.Error);
            }

            var payment = new CreatePaymentGrpcRequest();
            payment.OrderId = order.Id.ToString();
            payment.Amount = amount.ToString();
            payment.PaymentTypeId = orderRequest.PaymentTypeId.ToString();
            payment.PaymentMethodId = orderRequest.PaymentMethodId.ToString();
            var paymentResponse = await _paymentGrpcServiceClient.CreatePaymentAsync(payment);
            if (!paymentResponse.Success)
            {
                // Rollback product stock
                foreach (var product in productStock.Products)
                {
                    product.Quantity = -product.Quantity;
                }
                await _productGrpcServiceClient.VerifyAndUpdateProductStockAsync(productStock);
                throw new Exception(paymentResponse.Error);
            }
            
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw ex;
        }
    }
}