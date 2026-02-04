using System.Linq.Expressions;
using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.ICache;
using ProductService.Domain.ILogging;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;
using ProductService.Infrastructure.Constants;

namespace ProductService.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<ICache> _cacheMock = new();
    
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    
    private readonly Mock<IHttpClientFactory> _httpFactoryMock = new();
    private readonly Mock<IAppLogger<Application.Services.ProductService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    
    private readonly Application.Services.ProductService _service;
    
    public ProductServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.CategoryRepository)
            .Returns(_categoryRepoMock.Object);

        _unitOfWorkMock.Setup(x => x.ProductRepository)
            .Returns(_productRepoMock.Object);

        _service = new Application.Services.ProductService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _httpFactoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }
    
    private Product GetProduct(Guid id, Guid  categoryId)
    {
        return new Product()
        {
            Name = "Phone",
            Description = "Phone",
            CategoryId = categoryId,
            SellingPrice = 120000,
            Stock = 5,
            IsActive = true,
            BuyPrice = 100000,
            Id = id,
            Pictures =
            [
                new()
                {
                    Type = 1,
                    Url = "dummy"
                }
            ],
            Category = new Category()
            {
                Name = "Electronics",
                Id = categoryId
            }
        };
    }

    private IEnumerable<Product> GetProducts(Guid id, Guid categoryId)
    {
        return new List<Product>()
        {
            new()
            {
                Name = "Phone",
                Description = "Phone",
                CategoryId = categoryId,
                SellingPrice = 120000,
                Stock = 5,
                IsActive = true,
                BuyPrice = 100000,
                Id = id,
                Pictures =
                [
                    new Picture()
                    {
                        Url = "dummy"
                    }
                ],
                Category = new Category()
                {
                    Name = "Electronics",
                    Id = categoryId
                }
            }
        };
    }

    private ProductResponse GetProductResponse(Guid id, Guid categoryId)
    {
        return new ProductResponse()
        {
            Name = "Phone",
            Description = "Phone",
            SellingPrice = 120000,
            Stock = 5,
            IsActive = true,
            Id = id,
            Pictures =
            [
                new()
                {
                    Url = "dummy"
                }
            ],
            Category = new CategoryResponse()
            {
                Name = "Electronics",
                Id = categoryId
            }
        };
    }

    private ProductRequest GetProductRequest(Guid  categoryId)
    {
        return new ProductRequest()
        {
            Name = "Phone",
            Description = "Phone",
            CategoryId = categoryId,
            SellingPrice = 120000,
            Stock = 5,
            BuyPrice = 100000,
            Pictures =
            [
                new()
                {
                    Type = 1,
                    Url = "dummy"
                }
            ]
        };
    }

    [Fact]
    public async Task CreateProductAsync_Should_Create_Product()
    {
        var categoryId = Guid.NewGuid();
        var request = GetProductRequest(categoryId);

        var category = new Category
        {
            Id = categoryId,
            Name = "Test"
        };

        _categoryRepoMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
        
        var productResponse = new ProductResponse();
        _mapperMock
            .Setup(x => x.Map<ProductResponse>(It.IsAny<Product>()))
            .Returns(productResponse);

        // Act
        await _service.CreateProductAsync(request);

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);

        _productRepoMock.Verify(x => x.CreateAsync(It.IsAny<Product>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        
        _cacheMock.Verify(x => x.SetJsonAsync(Constants.ProductCacheKeyPrefix + productResponse.Id, 
            productResponse, It.IsAny<int?>()));

        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_WhenRepositoryFails_ShouldRollback_AndLogError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = GetProductRequest(categoryId);

        _categoryRepoMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Name = "Test", Id = categoryId });

        _productRepoMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("DB failure"));

        // Act + Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.CreateProductAsync(request));

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);

        _loggerMock.Verify(x => x.LogError(
                It.Is<Exception>(ex => ex.Message == "DB failure"),
                It.Is<string>(s => s == "Failed to create product")),
            Times.Once);
    }

    [Fact]
    public async Task GetAllProductsFromRedisAsync_Should_Return_Paginated_Response()
    {
        // Arrange
        var filter = new GetAllProductsFilter
        {
            IsActive = true,
            PageNumber = 1,
            ItemsPerPage = 10
        };

        var products = new List<ProductResponse>
        {
            new() { Name = "Phone", SellingPrice = 100, IsActive = true }
        };

        _cacheMock
            .Setup(x => x.GetAllJsonAsync<ProductResponse>(
                Constants.ProductCacheIndex,
                "@IsActive:{True} ",
                1, 10, "CreatedAt", "DESC"))
            .ReturnsAsync(products);

        _cacheMock
            .Setup(x => x.GetAllCountAsync(Constants.ProductCacheIndex, "@IsActive:{True} "))
            .ReturnsAsync(1);

        // Act
        var result = await _service.GetAllProductsFromRedisAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Phone", result.Data.First().Name);

        _cacheMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetAllProductsFromRedisAsync_Should_Build_Query_With_Category_And_Name()
    {
        // Arrange
        var filter = new GetAllProductsFilter
        {
            IsActive = true,
            CategoryName = "Electronics",
            Name = "Phone",
            PageNumber = 2,
            ItemsPerPage = 5
        };

        var expectedQuery =
            "@IsActive:{True} " +
            "@CategoryName:Electronics " +
            "(@Name:Phone* | @Name:Phone)";

        _cacheMock
            .Setup(x => x.GetAllJsonAsync<ProductResponse>(
                Constants.ProductCacheIndex,
                expectedQuery,
                2, 5, "CreatedAt", "DESC"))
            .ReturnsAsync(new List<ProductResponse>());

        _cacheMock
            .Setup(x => x.GetAllCountAsync(Constants.ProductCacheIndex, expectedQuery))
            .ReturnsAsync(0);

        // Act
        await _service.GetAllProductsFromRedisAsync(filter);

        // Assert
        _cacheMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetAllProductsFromRedisAsync_Should_Return_Empty_When_No_Products()
    {
        var filter = new GetAllProductsFilter
        {
            IsActive = false,
            PageNumber = 1,
            ItemsPerPage = 10
        };

        _cacheMock.Setup(x =>
                x.GetAllJsonAsync<ProductResponse>(It.IsAny<string>(), It.IsAny<string>(),
                    1, 10, "CreatedAt", "DESC"))
            .ReturnsAsync(new List<ProductResponse>());

        _cacheMock.Setup(x =>
                x.GetAllCountAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(0);

        var result = await _service.GetAllProductsFromRedisAsync(filter);

        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalItems);
    }
    
    [Fact]
    public async Task GetProductByIdAsync_Should_Return_Product_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        var product = GetProductResponse(id, Guid.Empty);

        var json = JsonSerializer.Serialize(product);

        _cacheMock
            .Setup(x => x.GetJsonAsync(Constants.ProductCacheKeyPrefix + id))
            .ReturnsAsync(json);

        // Act
        var result = await _service.GetProductByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Phone", result.Name);

        _cacheMock.Verify(x =>
                x.GetJsonAsync(Constants.ProductCacheKeyPrefix + id),
            Times.Once);
    }
    
    [Fact]
    public async Task GetProductByIdAsync_Should_Throw_When_Product_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _cacheMock
            .Setup(x => x.GetJsonAsync(Constants.ProductCacheKeyPrefix + id))
            .ReturnsAsync((string?)null);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.GetProductByIdAsync(id));

        Assert.Contains("not found", ex.Message);
    }
    
    [Fact]
    public async Task GetProductByIdAsync_Should_Log_Error_And_Rethrow_When_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();

        _cacheMock
            .Setup(x => x.GetJsonAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Redis failure"));

        // Act + Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.GetProductByIdAsync(id));

        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>()),
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateProductAsync_Should_Update_Product_And_Commit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = GetProduct(id, categoryId);

        var request = new ProductUpdateRequest
        {
            Name = "Phone",
            Description = "Phone",
            CategoryId = categoryId,
            SellingPrice = 120000,
            Stock = 5,
            BuyPrice = 100000,
            Pictures =
            [
                new()
                {
                    Type = 1,
                    Url = "dummy"
                }
            ],
            DeletePictureIds = new HashSet<Guid>()
        };

        _productRepoMock
            .Setup(x => x.GetAsync(
                It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                It.IsAny<IEnumerable<Expression<Func<Product, object>>>>()))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ProductResponse>(It.IsAny<Product>()))
            .Returns(new ProductResponse());

        // Act
        await _service.UpdateProductAsync(id, request);

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _productRepoMock.Verify(x => x.UpdateAsync(product), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        _cacheMock.Verify(x =>
                x.SetJsonAsync(It.IsAny<string>(), It.IsAny<ProductResponse>(), It.IsAny<int?>()),
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateProductAsync_Should_Throw_When_Product_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepoMock
            .Setup(x => x.GetAsync(
                It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                It.IsAny<IEnumerable<Expression<Func<Product, object>>>>()))
            .ReturnsAsync(It.IsAny<Product>());

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.UpdateProductAsync(id, new ProductUpdateRequest()));
        
        // Assert
        Assert.Equal($"Product with id {id} not found", ex.Message);

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task UpdateProductAsync_Should_Throw_When_More_Than_Five_Pictures()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = id,
            Name = "Phone",
            Description = "Phone",
            CategoryId = categoryId,
            SellingPrice = 120000,
            Stock = 5,
            BuyPrice = 100000,
            Pictures = [new(), new(), new(), new(), new()]
        };

        var request = new ProductUpdateRequest
        {
            Pictures = [new PictureRequest() {Type = 1, Url = "dummy"}],
            DeletePictureIds = new HashSet<Guid>()
        };

        _productRepoMock
            .Setup(x => x.GetAsync(
                It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                It.IsAny<IEnumerable<Expression<Func<Product, object>>>>()))
            .ReturnsAsync(product);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateProductAsync(id, request));

        // Assert
        Assert.Equal("You are not allowed to upload more than five pictures!", ex.Message);
    }
    
    [Fact]
    public async Task UpdateProductAsync_Should_Rollback_When_Exception_Occurs()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = GetProduct(id, categoryId);

        _productRepoMock
            .Setup(x => x.GetAsync(
                It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                It.IsAny<IEnumerable<Expression<Func<Product, object>>>>()))
            .ReturnsAsync(product);

        _productRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("db fail"));

        // Act
        Func<Task> act = async () =>
            await _service.UpdateProductAsync(id, new ProductUpdateRequest());

        // Assert
        await act.Should().ThrowAsync<Exception>();

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteProductAsync_Should_Delete_Product_And_Commit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = GetProduct(id, categoryId);

        _productRepoMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(product);

        // Act
        await _service.DeleteProductAsync(id);

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _productRepoMock.Verify(x => x.UpdateAsync(product), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        _cacheMock.Verify(x =>
                x.DeleteJsonAsync(It.IsAny<string>()),
            Times.Once);
    }
    
    [Fact]
    public async Task DeleteProductAsync_Should_Throw_When_Product_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepoMock
            .Setup(x => x.GetAsync(
                It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                It.IsAny<IEnumerable<Expression<Func<Product, object>>>>()))
            .ReturnsAsync(It.IsAny<Product>());

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.DeleteProductAsync(id));
        
        // Assert
        Assert.Equal($"Product with id {id} not found", ex.Message);

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteProductAsync_Should_Rollback_When_Exception_Occurs()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product = GetProduct(id, categoryId);

        _productRepoMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(product);

        _productRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("db fail"));

        // Act
        Func<Task> act = async () =>
            await _service.DeleteProductAsync(id);

        // Assert
        await act.Should().ThrowAsync<Exception>();

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task VerifyAndUpdateProductStockAsync_Should_Update_Product_And_Commit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var productStocks = new List<UpdateProductStockRequest>()
        {
            new UpdateProductStockRequest()
            {
                Id = id,
                Price = 120000,
                Quantity = 2,
            }
        };

        var products = GetProducts(id, categoryId);

        _productRepoMock
           .Setup<Task<List<Product>>>(
               x => x.GetAllAsync(
                   It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                   It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>?>()
               )
           )
           .ReturnsAsync((List<Product>)products);

        // Act
        await _service.VerifyAndUpdateProductStockAsync(productStocks);

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _productRepoMock.Verify(x => x.UpdateRangeAsync(products), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        _cacheMock.Verify(x =>
                x.SetJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()),
            Times.Once);
    }
    
    [Fact]
    public async Task VerifyAndUpdateProductStockAsync_Should_Throw_When_Products_Not_Available()
    {
        // Arrange
        var id = Guid.NewGuid();

        var productStocks = new List<UpdateProductStockRequest>()
        {
            new UpdateProductStockRequest()
            {
                Id = id,
                Price = 120000,
                Quantity = 2,
            }
        };

        _productRepoMock
            .Setup<Task<List<Product>>>(
                x => x.GetAllAsync(
                    It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>?>()
                )
            )
            .ReturnsAsync([]);

        // Act
        Func<Task> act = async () =>await _service.VerifyAndUpdateProductStockAsync(productStocks);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Some products are not available!");
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task VerifyAndUpdateProductStockAsync_Should_Throw_When_Price_Does_Not_Match()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var productStocks = new List<UpdateProductStockRequest>()
        {
            new UpdateProductStockRequest()
            {
                Id = id,
                Price = 120000,
                Quantity = 2,
            }
        };
        
        var products = new List<Product>()
        {
            new Product()
            {
                Id = id,
                Name = "Phone",
                Description = "Phone",
                CategoryId = categoryId,
                SellingPrice = 130000,
                Stock = 100,
                BuyPrice = 100000,
                Pictures =
                [
                    new()
                    {
                        Type = 1,
                        Url = "dummy"
                    }
                ]
            }
        };

        _productRepoMock
            .Setup<Task<List<Product>>>(
                x => x.GetAllAsync(
                    It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>?>()
                )
            )
            .ReturnsAsync(products);

        // Act
        Func<Task> act = async () =>await _service.VerifyAndUpdateProductStockAsync(productStocks);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Price of some products doesn't match!");
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task VerifyAndUpdateProductStockAsync_Should_Throw_When_Out_Of_Stock()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var productStocks = new List<UpdateProductStockRequest>()
        {
            new UpdateProductStockRequest()
            {
                Id = id,
                Price = 130000,
                Quantity = 2,
            }
        };
        
        var products = new List<Product>()
        {
            new()
            {
                Id = id,
                Name = "Phone",
                Description = "Phone",
                CategoryId = categoryId,
                SellingPrice = 130000,
                Stock = 0,
                BuyPrice = 100000,
                Pictures =
                [
                    new()
                    {
                        Type = 1,
                        Url = "dummy"
                    }
                ]
            }
        };

        _productRepoMock
            .Setup<Task<List<Product>>>(
                x => x.GetAllAsync(
                    It.IsAny<IEnumerable<Expression<Func<Product, bool>>>>(),
                    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>?>()
                )
            )
            .ReturnsAsync(products);

        // Act
        Func<Task> act = async () =>await _service.VerifyAndUpdateProductStockAsync(productStocks);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Some products are out of stock!");
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
}