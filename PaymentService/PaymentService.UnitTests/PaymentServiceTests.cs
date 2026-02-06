using AutoMapper;
using FluentAssertions;
using Moq;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.ILogging;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.UnitTests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IPaymentTypeRepository>  _paymentTypeRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAppLogger<Application.Services.PaymentService>> _loggerMock = new();
    private readonly Application.Services.PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.PaymentRepository).Returns(_paymentRepository.Object);
        _unitOfWorkMock.Setup(x => x.PaymentTypeRepository).Returns(_paymentTypeRepository.Object);
        _paymentService = new Application.Services.PaymentService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task GetAllPaymentsAsync_should_Return_Paginated_Response()
    {
        // Arrange
        var filter = new GetAllPaymentsFilter()
        {
            IsActive = true
        };

        var payments = new List<Payment>()
        {
            new()
            {
                Id = Guid.Empty,
                Amount = 240000,
                OrderId = Guid.Empty,
                PaymentTypeId = Guid.Empty,
                PaymentMethodId = Guid.Empty
            }
        };

        var paymentResponses = new List<PaymentResponse>()
        {
            new()
            {
                Id = Guid.Empty,
                Amount = 240000,
                OrderId = Guid.Empty,
                PaymentTypeId = Guid.Empty,
                PaymentMethodId = Guid.Empty
            }
        };
        
        _paymentRepository.Setup(x => x.GetAllPaymentsAsync(
            It.IsAny<bool>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<Guid?>(),
            It.IsAny<int>(),
            It.IsAny<int>()
        )).ReturnsAsync(payments);

        _paymentRepository.Setup(x => x.GetAllPaymentCountAsync(
                It.IsAny<bool>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<Guid?>()
                )).ReturnsAsync(1);
        
        _mapperMock.Setup(x => x.Map<IEnumerable<PaymentResponse>>(payments)).Returns(paymentResponses);
        
        // Act
        var result = await _paymentService.GetAllPaymentsAsync(filter);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(240000, result.Data.First().Amount);
        _paymentRepository.VerifyAll();
    }
    
    [Fact]
    public async Task GetPaymentByIdAsync_should_Return_Payment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payment = new Payment()
        {
            Id = Guid.Empty,
            Amount = 240000,
            OrderId = Guid.Empty,
            PaymentTypeId = Guid.Empty,
            PaymentMethodId = Guid.Empty
        };
        var paymentResponse = new PaymentResponse()
        {
            Id = Guid.Empty,
            Amount = 240000,
            OrderId = Guid.Empty,
            PaymentTypeId = Guid.Empty,
            PaymentMethodId = Guid.Empty
        };
        
        _paymentRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(payment);
        
        _mapperMock.Setup(x => x.Map<PaymentResponse>(payment)).Returns(paymentResponse);
        
        // Act
        var result = await _paymentService.GetPaymentByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        _paymentRepository.VerifyAll();
    }
    
    [Fact]
    public async Task GetPaymentByIdAsync_should_Throw_Payment_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        _paymentRepository.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment with id {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentService.GetPaymentByIdAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment with id {id} not found");
        _paymentRepository.VerifyAll();
    }
    
    [Fact]
    public async Task CreatePaymentAsync_Should_Create_Payment()
    {
        // Arrange
        var paymentTypeId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var paymentRequest = new PaymentRequest()
        {
            Amount = 240000,
            OrderId = Guid.Empty,
            PaymentTypeId = paymentTypeId,
            PaymentMethodId = paymentMethodId
        };
        var payment = new Payment()
        {
            Id = Guid.Empty,
            Amount = 240000,
            OrderId = Guid.Empty,
            PaymentTypeId = paymentTypeId,
            PaymentMethodId = paymentMethodId
        };
        _paymentTypeRepository.Setup(x =>
            x.PaymentTypeMatchesWithPaymentMethodAsync(
                paymentTypeId, 
                paymentMethodId
                )).ReturnsAsync(true);
        _mapperMock.Setup(x => x.Map<Payment>(paymentRequest)).Returns(payment);
        _paymentRepository.Setup(x => x.CreateAsync(payment))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentService.CreatePaymentAsync(paymentRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentRepository.VerifyAll();
        _paymentTypeRepository.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePaymentAsync_Should_Update_Payment()
    {
        // Arrange
        var paymentTypeId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var paymentRequest = new PaymentRequest()
        {
            Amount = 240000,
            OrderId = Guid.Empty,
            PaymentTypeId = paymentTypeId,
            PaymentMethodId = paymentMethodId
        };
        var payment = new Payment()
        {
            Id = Guid.Empty,
            Amount = 240000,
            OrderId = Guid.Empty,
            PaymentTypeId = paymentTypeId,
            PaymentMethodId = paymentMethodId
        };
        _paymentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(payment);
        _mapperMock.Setup(x => x.Map(paymentRequest, payment)).Returns(payment);
        _paymentRepository.Setup(x => x.UpdateAsync(payment))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentService.UpdatePaymentAsync(It.IsAny<Guid>(), paymentRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentRepository.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePaymentAsync_should_Throw_Payment_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        _paymentRepository.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment with id {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentService.UpdatePaymentAsync(id, It.IsAny<PaymentRequest>());
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment with id {id} not found");
        _paymentRepository.VerifyAll();
    }
    
    [Fact]
    public async Task DeletePaymentAsync_Should_Delete_Payment()
    {
        // Arrange
        var payment = new Payment()
        {
            Id = Guid.Empty,
            Amount = 240000,
            OrderId = Guid.Empty,
            PaymentTypeId = Guid.Empty,
            PaymentMethodId = Guid.Empty
        };
        _paymentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(payment);
        _paymentRepository.Setup(x => x.UpdateAsync(payment))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentService.DeletePaymentAsync(It.IsAny<Guid>());
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentRepository.VerifyAll();
    }
    
    [Fact]
    public async Task DeletePaymentAsync_should_Throw_Payment_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        _paymentRepository.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment with id {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentService.DeletePaymentAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment with id {id} not found");
        _paymentRepository.VerifyAll();
    }
}