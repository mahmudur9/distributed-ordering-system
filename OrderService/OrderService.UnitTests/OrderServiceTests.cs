using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Grpc.Core;
using Moq;
using OrderService.Application.IServices;
using OrderService.Application.Requests;
using OrderService.Application.Responses;
using OrderService.Domain.ILogging;
using OrderService.Domain.IRepositories;
using OrderService.Domain.Models;
using PaymentService.API;
using ProductService.API;

namespace OrderService.UnitTests;

public class OrderServiceTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IAppLogger<Application.Services.OrderService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Application.Services.OrderService _orderService;
    private readonly Mock<PaymentGrpcService.PaymentGrpcServiceClient> _paymentGrpcServiceClientMock = new();
    private readonly Mock<ProductGrpcService.ProductGrpcServiceClient> _productGrpcServiceClientMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public OrderServiceTests()
    {
        _unitOfWorkMock.Setup(uow => uow.OrderRepository).Returns(_orderRepositoryMock.Object);
        _orderService = new Application.Services.OrderService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _paymentGrpcServiceClientMock.Object,
            _productGrpcServiceClientMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object
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

        var grpcResponse = new GrpcResponse
        {
            Success = true
        };
        var productStockRequest = new UpdateProductGrpcStockRequest();
        productStockRequest.Products.Add(new ProductStockGrpcRequest
        {
            Id = Guid.Empty.ToString(),
            Quantity = 2,
            Price = "120000"
        });
        var asyncGrpcResponse = new AsyncUnaryCall<GrpcResponse>(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );
        _productGrpcServiceClientMock.Setup<AsyncUnaryCall<GrpcResponse>>(x => x.VerifyAndUpdateProductStockAsync(
            productStockRequest,
            It.IsAny<Metadata>(),
            null,
            It.IsAny<CancellationToken>()
        )).Returns(asyncGrpcResponse);

        var paymentGrpcResponse = new CreatePaymentGrpcResponse
        {
            Success = true
        };
        var asyncPaymentGrpcResponse = new AsyncUnaryCall<CreatePaymentGrpcResponse>(
            Task.FromResult(paymentGrpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );
        _paymentGrpcServiceClientMock.Setup<AsyncUnaryCall<CreatePaymentGrpcResponse>>(x => x.CreatePaymentAsync(
            It.IsAny<CreatePaymentGrpcRequest>(),
            It.IsAny<Metadata>(),
            null,
            It.IsAny<CancellationToken>()
        )).Returns(asyncPaymentGrpcResponse);

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
        
        var productStockRequest = new UpdateProductGrpcStockRequest();
        productStockRequest.Products.Add(new ProductStockGrpcRequest
        {
            Id = Guid.Empty.ToString(),
            Quantity = 2,
            Price = "120000"
        });
        _productGrpcServiceClientMock.Setup<AsyncUnaryCall<GrpcResponse>>(x => x.VerifyAndUpdateProductStockAsync(
            productStockRequest,
            It.IsAny<Metadata>(),
            null,
            It.IsAny<CancellationToken>()
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

        var grpcResponse = new GrpcResponse
        {
            Success = false,
            Error = "error"
        };
        var productStockRequest = new UpdateProductGrpcStockRequest();
        productStockRequest.Products.Add(new ProductStockGrpcRequest
        {
            Id = Guid.Empty.ToString(),
            Quantity = 2,
            Price = "120000"
        });
        var asyncGrpcResponse = new AsyncUnaryCall<GrpcResponse>(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );
        _productGrpcServiceClientMock.Setup<AsyncUnaryCall<GrpcResponse>>(x => x.VerifyAndUpdateProductStockAsync(
            productStockRequest,
            It.IsAny<Metadata>(),
            null,
            It.IsAny<CancellationToken>()
        )).Returns(asyncGrpcResponse);
       
        _paymentGrpcServiceClientMock.Setup<AsyncUnaryCall<CreatePaymentGrpcResponse>>(x => x.CreatePaymentAsync(
            It.IsAny<CreatePaymentGrpcRequest>(),
            It.IsAny<Metadata>(),
            null,
            It.IsAny<CancellationToken>()
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
}