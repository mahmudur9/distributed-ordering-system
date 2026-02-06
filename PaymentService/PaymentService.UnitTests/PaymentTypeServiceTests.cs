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

public class PaymentTypeServiceTests
{
    private readonly Mock<IPaymentTypeRepository> _paymentTypeRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAppLogger<PaymentTypeService>> _loggerMock = new();
    private readonly PaymentTypeService _paymentTypeService;

    public PaymentTypeServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.PaymentTypeRepository).Returns(_paymentTypeRepositoryMock.Object);
        _paymentTypeService = new PaymentTypeService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllPaymentTypesAsync_should_Return_Paginated_Response()
    {
        // Arrange
        var filter = new GetAllPaymentTypesFilter()
        {
            Name =  "test"
        };

        var paymentTypes = new List<PaymentType>()
        {
            new()
            {
                Name = "Test"
            }
        };

        var paymentTypeResponses = new List<PaymentTypeResponse>()
        {
            new()
            {
                Name = "Test"
            }
        };
        
        _paymentTypeRepositoryMock.Setup<Task<IEnumerable<PaymentType>>>(x => x.GetAllPaymentTypesAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
                It.IsAny<int>(),
            It.IsAny<int>()
            )).ReturnsAsync(paymentTypes);

        _paymentTypeRepositoryMock.Setup(x => x.GetAllPaymentTypeCountAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(1);
        
        _mapperMock.Setup(x => x.Map<IEnumerable<PaymentTypeResponse>>(paymentTypes)).Returns(paymentTypeResponses);
        
        // Act
        var result = await _paymentTypeService.GetAllPaymentTypesAsync(filter);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Test", result.Data.First().Name);
        _paymentTypeRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetPaymentTypeByIdAsync_should_Return_PaymentType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentType = new PaymentType()
        {
            Id = id,
            Name = "Test"
        };
        var paymentTypeResponse = new PaymentTypeResponse()
        {
            Id = id,
            Name = "Test"
        };
        
        _paymentTypeRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(paymentType);
        
        _mapperMock.Setup(x => x.Map<PaymentTypeResponse>(paymentType)).Returns(paymentTypeResponse);
        
        // Act
        var result = await _paymentTypeService.GetPaymentTypeByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        _paymentTypeRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetPaymentTypeByIdAsync_should_Throw_PaymentType_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        _paymentTypeRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment Type with {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentTypeService.GetPaymentTypeByIdAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment Type with {id} not found");
        _paymentTypeRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task CreatePaymentTypeAsync_Should_Create_PaymentType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentType = new PaymentType()
        {
            Id = id,
            Name = "Test"
        };
        var paymentTypeRequest = new PaymentTypeRequest()
        {
            Name = "Test"
        };
        _mapperMock.Setup(x => x.Map<PaymentType>(paymentTypeRequest)).Returns(paymentType);
        _paymentTypeRepositoryMock.Setup(x => x.CreateAsync(paymentType))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentTypeService.CreatePaymentTypeAsync(paymentTypeRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentTypeRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePaymentTypeAsync_Should_Update_PaymentType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentType = new PaymentType()
        {
            Id = id,
            Name = "Test"
        };
        var paymentTypeRequest = new PaymentTypeRequest()
        {
            Name = "Test"
        };
        _paymentTypeRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(paymentType);
        _mapperMock.Setup(x => x.Map(paymentTypeRequest, paymentType)).Returns(paymentType);
        _paymentTypeRepositoryMock.Setup(x => x.UpdateAsync(paymentType))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentTypeService.UpdatePaymentTypeAsync(id, paymentTypeRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentTypeRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePaymentTypeAsync_Should_Throw_PaymentType_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentTypeRequest = new PaymentTypeRequest()
        {
            Name = "Test"
        };
        _paymentTypeRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment Type with {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentTypeService.UpdatePaymentTypeAsync(id, paymentTypeRequest);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment Type with {id} not found");
        _paymentTypeRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeletePaymentTypeAsync_Should_Delete_PaymentType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentType = new PaymentType()
        {
            Id = id,
            Name = "Test"
        };
        _paymentTypeRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(paymentType);
        _paymentTypeRepositoryMock.Setup(x => x.UpdateAsync(paymentType))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentTypeService.DeletePaymentTypeAsync(id);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentTypeRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeletePaymentTypeAsync_Should_Throw_When_PaymentType_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        _paymentTypeRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Payment Type with {id} not found"));
        
        // Act
        Func<Task> act = async () => await _paymentTypeService.DeletePaymentTypeAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Payment Type with {id} not found");
        _paymentTypeRepositoryMock.VerifyAll();
    }
}