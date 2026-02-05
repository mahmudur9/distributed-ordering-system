using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using OrderService.Application.Abstractions.Gateways;
using OrderService.Application.Abstractions.Logging;
using OrderService.Application.IServices;
using OrderService.Application.Requests;
using OrderService.Application.Responses;
using OrderService.Domain.IRepositories;
using OrderService.Domain.Models;

namespace OrderService.UnitTests;

public class OrderServiceTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IAppLogger<Application.Services.OrderService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IPaymentGateway> _paymentGatewayMock = new();
    private readonly Mock<IProductGateway> _productGatewayMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Application.Services.OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock.Setup(uow => uow.OrderRepository).Returns(_orderRepositoryMock.Object);
        _orderService = new Application.Services.OrderService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object,
            _productGatewayMock.Object,
            _paymentGatewayMock.Object
        );
    }

    [Fact]
    public async Task GetAllOrdersAsync_Should_Return_Paginated_Response()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                UserId = Guid.Empty,
                Amount = 240000,
                Products =
                [
                    new ProductOrder
                    {
                        ProductId = Guid.Empty,
                        ProductPrice = 120000,
                        Quantity = 2
                    }
                ]
            }
        };
        var orderResponses = new List<OrderResponse>
        {
            new()
            {
                UserId = Guid.Empty,
                Amount = 240000,
                Products = new List<ProductOrderResponse>
                {
                    new()
                    {
                        ProductId = Guid.Empty,
                        ProductPrice = 120000,
                        Quantity = 2
                    }
                }
            }
        };
        var filter = new GetAllOrdersFilter
        {
            IsActive = true,
            DateFrom = null,
            DateTo = null
        };
        _mapperMock.Setup(m => m.Map<IEnumerable<OrderResponse>>(It.IsAny<IEnumerable<Order>>()))
            .Returns(orderResponses);

        _orderRepositoryMock.Setup<Task<List<Order>>>(x => x.GetAllAsync(
            It.IsAny<IEnumerable<Expression<Func<Order, bool>>>>(),
            It.IsAny<IEnumerable<Expression<Func<Order, object>>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>?>()
        )).ReturnsAsync(orders);

        _orderRepositoryMock.Setup<Task<int>>(x => x.CountAsync(
            It.IsAny<IEnumerable<Expression<Func<Order, bool>>>>()
        )).ReturnsAsync(1);

        // Act
        var result = await _orderService.GetAllOrdersAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(240000, result.Data.First().Amount);
        _orderRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllOrdersAsync_Should_Throw_When_Exception_Occurs()
    {
        // Arrange
        var filter = new GetAllOrdersFilter
        {
            IsActive = true,
            DateFrom = null,
            DateTo = null
        };

        _orderRepositoryMock.Setup<Task<List<Order>>>(x => x.GetAllAsync(
            It.IsAny<IEnumerable<Expression<Func<Order, bool>>>>(),
            It.IsAny<IEnumerable<Expression<Func<Order, object>>>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>?>()
        )).ThrowsAsync(new Exception("db fail"));

        // Act
        Func<Task> act = async () => await _orderService.GetAllOrdersAsync(filter);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _orderRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Create_Order_And_Commit()
    {
        // Arrange
        var order = new Order
        {
            UserId = Guid.Empty,
            Amount = 240000,
            Products =
            [
                new ProductOrder
                {
                    ProductName = "Test",
                    ProductId = Guid.Empty,
                    ProductPrice = 120000,
                    Quantity = 2
                }
            ]
        };
        var orderRequest = new OrderRequest
        {
            PaymentTypeId = Guid.Empty,
            PaymentMethodId = Guid.Empty,
            Products = new List<ProductOrderRequest>
            {
                new()
                {
                    ProductId = Guid.Empty,
                    ProductPrice = 120000,
                    Quantity = 2
                }
            }
        };

        _mapperMock.Setup(m => m.Map<Order>(It.IsAny<OrderRequest>())).Returns(order);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(Guid.NewGuid());
        
        var gatewayResponse = new GatewayResponse
        {
            Success = true,
            Error = ""
        };
        
        _productGatewayMock.Setup<Task<GatewayResponse>>(x => x.VerifyAndUpdateProductStockAsync(
                    It.IsAny<List<ProductStockGatewayRequest>>()
                )).ReturnsAsync(gatewayResponse);
        
        _paymentGatewayMock.Setup<Task<GatewayResponse>>(x => x.CreatePaymentAsync(
            It.IsAny<CreatePaymentGatewayRequest>()
        )).ReturnsAsync(gatewayResponse);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

        // Act
        await _orderService.CreateOrderAsync(orderRequest);

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CreateOrderAsync_Should_Throw_When_Stock_Verifying_Fails()
    {
        // Arrange
        var order = new Order
        {
            UserId = Guid.Empty,
            Amount = 240000,
            Products =
            [
                new ProductOrder
                {
                    ProductId = Guid.Empty,
                    ProductPrice = 120000,
                    Quantity = 2
                }
            ]
        };
        var orderRequest = new OrderRequest
        {
            PaymentTypeId = Guid.Empty,
            PaymentMethodId = Guid.Empty,
            Products = new List<ProductOrderRequest>
            {
                new()
                {
                    ProductId = Guid.Empty,
                    ProductPrice = 120000,
                    Quantity = 2
                }
            }
        };

        _mapperMock.Setup(m => m.Map<Order>(It.IsAny<OrderRequest>())).Returns(order);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(Guid.NewGuid());
        
        _productGatewayMock.Setup<Task<GatewayResponse>>(x => x.VerifyAndUpdateProductStockAsync(
            It.IsAny<List<ProductStockGatewayRequest>>()
        )).Throws(new  Exception("error"));

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _orderService.CreateOrderAsync(orderRequest);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("error");
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CreateOrderAsync_Should_Throw_When_Payment_Fails()
    {
        // Arrange
        var order = new Order
        {
            UserId = Guid.Empty,
            Amount = 240000,
            Products =
            [
                new ProductOrder
                {
                    ProductId = Guid.Empty,
                    ProductPrice = 120000,
                    Quantity = 2
                }
            ]
        };
        var orderRequest = new OrderRequest
        {
            PaymentTypeId = Guid.Empty,
            PaymentMethodId = Guid.Empty,
            Products = new List<ProductOrderRequest>
            {
                new()
                {
                    ProductId = Guid.Empty,
                    ProductPrice = 120000,
                    Quantity = 2
                }
            }
        };

        _mapperMock.Setup(m => m.Map<Order>(It.IsAny<OrderRequest>())).Returns(order);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(Guid.NewGuid());
        
        var gatewayResponse = new GatewayResponse
        {
            Success = true,
            Error = ""
        };
        
        _productGatewayMock.Setup<Task<GatewayResponse>>(x => x.VerifyAndUpdateProductStockAsync(
            It.IsAny<List<ProductStockGatewayRequest>>()
        )).ReturnsAsync(gatewayResponse);
       
        _paymentGatewayMock.Setup(x => x.CreatePaymentAsync(
            It.IsAny<CreatePaymentGatewayRequest>()
        )).ThrowsAsync(new  Exception("error"));

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _orderService.CreateOrderAsync(orderRequest);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("error");
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
}