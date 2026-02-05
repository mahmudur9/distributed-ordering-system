using System.Linq.Expressions;
using AutoMapper;
using OrderService.Application.Abstractions.Gateways;
using OrderService.Application.IServices;
using OrderService.Application.Requests;
using OrderService.Application.Responses;
using OrderService.Domain.ILogging;
using OrderService.Domain.IRepositories;
using OrderService.Domain.Models;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IProductGateway _productGateway;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IAuthService  _authService;
    private readonly IAppLogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, IAppLogger<OrderService> logger, IProductGateway productGateway, IPaymentGateway paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authService = authService;
        _logger = logger;
        _productGateway = productGateway;
        _paymentGateway = paymentGateway;
    }

    private List<ProductStockGatewayRequest> MapProductStock(OrderRequest orderRequest)
    {
        var productStocks = new List<ProductStockGatewayRequest>();
        foreach (var product in orderRequest.Products)
        {
            productStocks.Add(new ProductStockGatewayRequest()
            {
                Id = product.ProductId,
                Quantity = product.Quantity,
                Price = product.ProductPrice
            });
        }
        
        return productStocks;
    }

    public async Task CreateOrderAsync(OrderRequest orderRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new order");
            var order = _mapper.Map<Order>(orderRequest);
            decimal amount = 0;
            foreach (var product in orderRequest.Products)
            {
                amount += product.ProductPrice * product.Quantity;
            }
            order.Amount = amount;
            order.UserId = _authService.GetAuthenticatedUserId();
            
            await _unitOfWork.BeginTransactionAsync();
            
            await _unitOfWork.OrderRepository.CreateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var productStocks = MapProductStock(orderRequest);
            var productStockResponse = await _productGateway.VerifyAndUpdateProductStockAsync(productStocks);
            if (!productStockResponse.Success)
            {
                throw new ArgumentException(productStockResponse.Error);
            }

            var payment = new CreatePaymentGatewayRequest
            {
                OrderId = order.Id,
                Amount = amount,
                PaymentTypeId = orderRequest.PaymentTypeId,
                PaymentMethodId = orderRequest.PaymentMethodId ?? Guid.Empty
            };
            var paymentResponse = await _paymentGateway.CreatePaymentAsync(payment);
            if (!paymentResponse.Success)
            {
                // Rollback product stock
                foreach (var product in productStocks)
                {
                    product.Quantity = -product.Quantity;
                }
                await _productGateway.VerifyAndUpdateProductStockAsync(productStocks);
                throw new Exception(paymentResponse.Error);
            }
            
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to create order");
            throw;
        }
    }

    public async Task<PaginatedResponse<OrderResponse>> GetAllOrdersAsync(GetAllOrdersFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all orders");
            List<Expression<Func<Order, bool>>> filters = [];
            filters.Add(x => x.IsActive == filter.IsActive);
            if (filter.DateFrom is not null) filters.Add(x => x.CreatedAt >= filter.DateFrom);
            if (filter.DateTo is not null) filters.Add(x => x.CreatedAt <= filter.DateTo);

            var orders = await _unitOfWork.OrderRepository.GetAllAsync(filters, [x => x.Products], filter.ItemsPerPage,
                filter.PageNumber, x => x.OrderByDescending(order => order.CreatedAt));
            var productCount = await _unitOfWork.OrderRepository.CountAsync(filters);
            
            var paginatedResponse = new PaginatedResponse<OrderResponse>(
                _mapper.Map<IEnumerable<OrderResponse>>(orders),
                productCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all orders");
            throw;
        }
    }

    public async Task<PaginatedResponse<OrderResponse>> GetAllOrdersByUserIdAsync(GetAllOrdersFilter filter)
    {
        try
        {
            var userId = _authService.GetAuthenticatedUserId();
            _logger.LogInformation($"Getting all orders by userId {userId}");
            List<Expression<Func<Order, bool>>> filters = [];
            filters.Add(x => x.IsActive == filter.IsActive &&  x.UserId == userId);
            if (filter.DateFrom is not null) filters.Add(x => x.CreatedAt >= filter.DateFrom);
            if (filter.DateTo is not null) filters.Add(x => x.CreatedAt <= filter.DateTo);

            var orders = await _unitOfWork.OrderRepository.GetAllAsync(filters, [x => x.Products], filter.ItemsPerPage,
                filter.PageNumber, x => x.OrderByDescending(order => order.CreatedAt));
            var productCount = await _unitOfWork.OrderRepository.CountAsync(filters);
            
            var paginatedResponse = new PaginatedResponse<OrderResponse>(
                _mapper.Map<IEnumerable<OrderResponse>>(orders),
                productCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get all orders by userId {_authService.GetAuthenticatedUserId()}");
            throw;
        }
    }
}