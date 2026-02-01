using AutoMapper;
using Moq;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.ICache;
using ProductService.Domain.ILogging;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;

namespace ProductService.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<ICache> _cacheMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IHttpClientFactory> _httpFactoryMock = new();
    private readonly Mock<IAppLogger<Application.Services.ProductService>> _loggerMock = new();

    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();

    private readonly Application.Services.ProductService _service;
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public ProductServiceTests()
    {
        _uowMock.Setup(x => x.CategoryRepository)
            .Returns(_categoryRepoMock.Object);

        _uowMock.Setup(x => x.ProductRepository)
            .Returns(_productRepoMock.Object);

        _service = new Application.Services.ProductService(
            _uowMock.Object,
            _mapperMock.Object,
            _httpFactoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateProductAsync_Should_Create_Product()
    {
        var categoryId = Guid.NewGuid();
        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            SellingPrice = 100,
            BuyPrice = 80,
            Stock = 100,
            CategoryId = categoryId,
            Pictures = new List<PictureRequest>
            {
                new()
                {
                    Type = 1,
                    Url = "dummy"
                }
            }
        };

        var category = new Category
        {
            Id = categoryId,
            Name = "Test"
        };

        _categoryRepoMock
            .Setup(x => x.GetByIdAsync(request.CategoryId.Value))
            .ReturnsAsync(category);

        _uowMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

        // If MapToProduct uses mapper internally
        var productResponse = new ProductResponse();
        _mapperMock
            .Setup(x => x.Map<ProductResponse>(It.IsAny<Product>()))
            .Returns(productResponse);

        // Act
        await _service.CreateProductAsync(request);

        // Assert
        _uowMock.Verify(x => x.BeginTransactionAsync(), Times.Once);

        _productRepoMock.Verify(x => x.CreateAsync(It.IsAny<Product>()), Times.Once);

        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        _uowMock.Verify(x => x.CommitTransactionAsync(), Times.Once);

        _uowMock.Verify(x => x.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_WhenRepositoryFails_ShouldRollback_AndLogError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            SellingPrice = 100,
            BuyPrice = 80,
            Stock = 100,
            CategoryId = categoryId,
            Pictures = new List<PictureRequest>
            {
                new()
                {
                    Type = 1,
                    Url = "dummy"
                }
            }
        };

        _categoryRepoMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Name = "Test", Id = categoryId });

        _productRepoMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("DB failure"));

        // Act + Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.CreateProductAsync(request));

        _uowMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);

        _loggerMock.Verify(x => x.LogError(
                It.Is<Exception>(ex => ex.Message == "DB failure"),
                It.Is<string>(s => s == "Failed to create product")),
            Times.Once);
    }
}