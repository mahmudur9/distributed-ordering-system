using AutoMapper;
using FluentAssertions;
using Moq;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Application.Services;
using PaymentService.Domain.ILogging;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.UnitTests;

public class PaymentMethodServiceTests
{
    private readonly Mock<IPaymentMethodRepository> _paymentMethodRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAppLogger<PaymentMethodService>> _loggerMock = new();
    private readonly PaymentMethodService _paymentMethodService;

    public PaymentMethodServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.PaymentMethodRepository).Returns(_paymentMethodRepositoryMock.Object);
        _paymentMethodService = new PaymentMethodService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task GetAllPaymentMethodsAsync_should_Return_Paginated_Response()
    {
        // Arrange
        var filter = new GetAllPaymentMethodsFilter()
        {
            Name =  "test"
        };

        var paymentMethods = new List<PaymentMethod>()
        {
            new()
            {
                Name = "Test",
                PaymentTypeId = Guid.Empty
            }
        };

        var paymentMethodResponses = new List<PaymentMethodResponse>()
        {
            new()
            {
                Name = "Test"
            }
        };
        
        _paymentMethodRepositoryMock.Setup<Task<IEnumerable<PaymentMethod>>>(x => x.GetAllPaymentMethodsAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>()
        )).ReturnsAsync(paymentMethods);

        _paymentMethodRepositoryMock.Setup(x => x.GetAllPaymentMethodCountAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(1);
        
        _mapperMock.Setup(x => x.Map<IEnumerable<PaymentMethodResponse>>(paymentMethods)).Returns(paymentMethodResponses);
        
        // Act
        var result = await _paymentMethodService.GetAllPaymentMethodsAsync(filter);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Test", result.Data.First().Name);
        _paymentMethodRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetPaymentMethodByIdAsync_should_Return_PaymentMethod()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentMethod = new PaymentMethod()
        {
            Id = id,
            Name = "Test",
            PaymentTypeId = Guid.Empty
        };
        var paymentMethodResponse = new PaymentMethodResponse()
        {
            Id = id,
            Name = "Test"
        };
        
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(paymentMethod);
        
        _mapperMock.Setup(x => x.Map<PaymentMethodResponse>(paymentMethod)).Returns(paymentMethodResponse);
        
        // Act
        var result = await _paymentMethodService.GetPaymentMethodByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        _paymentMethodRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetPaymentMethodByIdAsync_should_Throw_PaymentMethod_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment Method with {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentMethodService.GetPaymentMethodByIdAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment Method with {id} not found");
        _paymentMethodRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task CreatePaymentMethodAsync_Should_Create_PaymentMethod()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentMethod = new PaymentMethod()
        {
            Id = id,
            Name = "Test",
            PaymentTypeId = Guid.Empty
        };
        var paymentMethodRequest = new PaymentMethodRequest()
        {
            Name = "Test",
            PaymentTypeId = Guid.Empty
        };
        _mapperMock.Setup(x => x.Map<PaymentMethod>(paymentMethodRequest)).Returns(paymentMethod);
        _paymentMethodRepositoryMock.Setup(x => x.CreateAsync(paymentMethod))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentMethodService.CreatePaymentMethodAsync(paymentMethodRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentMethodRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePaymentMethodAsync_Should_Update_PaymentMethod()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentMethod = new PaymentMethod()
        {
            Id = id,
            Name = "Test",
            PaymentTypeId = Guid.Empty
        };
        var paymentMethodRequest = new PaymentMethodRequest()
        {
            Name = "Test",
            PaymentTypeId = Guid.Empty
        };
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(paymentMethod);
        _mapperMock.Setup(x => x.Map(paymentMethodRequest, paymentMethod)).Returns(paymentMethod);
        _paymentMethodRepositoryMock.Setup(x => x.UpdateAsync(paymentMethod))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentMethodService.UpdatePaymentMethodAsync(id, paymentMethodRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentMethodRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePaymentMethodAsync_should_Throw_PaymentMethod_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentMethodRequest = new PaymentMethodRequest()
        {
            Name = "Test",
            PaymentTypeId = Guid.Empty
        };
        
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment Method with {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentMethodService.UpdatePaymentMethodAsync(id, paymentMethodRequest);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment Method with {id} not found");
        _paymentMethodRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeletePaymentMethodAsync_Should_Delete_PaymentMethod()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentMethod = new PaymentMethod()
        {
            Id = id,
            Name = "Test",
            PaymentTypeId = Guid.Empty
        };
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(paymentMethod);
        _paymentMethodRepositoryMock.Setup(x => x.UpdateAsync(paymentMethod))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentMethodService.DeletePaymentMethodAsync(id);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentMethodRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeletePaymentMethodAsync_should_Throw_PaymentMethod_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment Method with {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentMethodService.DeletePaymentMethodAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment Method with {id} not found");
        _paymentMethodRepositoryMock.VerifyAll();
    }
}