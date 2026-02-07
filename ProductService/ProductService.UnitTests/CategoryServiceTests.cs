using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Application.Services;
using ProductService.Domain.ILogging;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;

namespace ProductService.UnitTests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAppLogger<CategoryService>> _loggerMock = new();

    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.CategoryRepository)
            .Returns(_categoryRepoMock.Object);

        _unitOfWorkMock.Setup(x => x.ProductRepository)
            .Returns(_productRepoMock.Object);
        
        _service = new CategoryService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task GetAllCategoriesAsync_Should_Return_Paginated_Response()
    {
        // Arrange
        var filter = new GetAllCategoriesFilter()
        {
            Name = "Test",
            IsActive = true,
            PageNumber = 1,
            ItemsPerPage = 10
        };

        var id = Guid.NewGuid();
        
        var categoryResponses = new List<CategoryResponse>()
        {
            new() { Id = id, Name = "Test", IsActive = true }
        };
        
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryResponse>>(It.IsAny<List<Category>>())).Returns(categoryResponses);

        _categoryRepoMock
            .Setup<Task<List<Category>>>(x => x.GetAllAsync(
                It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>?>()
                )).ReturnsAsync(It.IsAny<List<Category>>());

        _categoryRepoMock
            .Setup<Task<int>>(x => x.CountAsync(
                It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>()
            )).ReturnsAsync(1);

        

        // Act
        var result = await _service.GetAllCategoriesAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Test", result.Data.First().Name);
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetAllCategoriesAsync_Should_Return_Empty_When_No_Products()
    {
        // Arrange
        var filter = new GetAllCategoriesFilter()
        {
            Name = "Test",
            IsActive = true,
            PageNumber = 1,
            ItemsPerPage = 10
        };
        
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryResponse>>(It.IsAny<List<Category>>())).Returns([]);

        _categoryRepoMock
            .Setup<Task<List<Category>>>(x => x.GetAllAsync(
                It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>?>()
            )).ReturnsAsync(It.IsAny<List<Category>>());

        _categoryRepoMock
            .Setup<Task<int>>(x => x.CountAsync(
                It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>()
            )).ReturnsAsync(0);

        

        // Act
        var result = await _service.GetAllCategoriesAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalItems);
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetAllCategoriesAsync_Should_Throw_When_Exception_Occurs()
    {
        // Arrange
        var filter = new GetAllCategoriesFilter()
        {
            Name = "Test",
            IsActive = true,
            PageNumber = 1,
            ItemsPerPage = 10
        };
        
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryResponse>>(It.IsAny<List<Category>>())).Returns([]);

        _categoryRepoMock
            .Setup<Task<List<Category>>>(x => x.GetAllAsync(
                It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>?>()
            )).ThrowsAsync(new Exception("db fail"));

        

        // Act
        Func<Task> act = async () => await _service.GetAllCategoriesAsync(filter);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetCategoryByIdAsync_Should_Return_Category()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category() { Id = id, Name = "Test", IsActive = true };
        var categoryResponse = new CategoryResponse() { Id = id, Name = "Test", IsActive = true };
        
        _mapperMock.Setup(x => x.Map<CategoryResponse>(It.IsAny<Category>())).Returns(categoryResponse);

        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(category);

        // Act
        var result = await _service.GetCategoryByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task GetCategoryByIdAsync_Should_Throw_Category_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        _mapperMock.Setup(x => x.Map<CategoryResponse>(It.IsAny<Category>())).Returns(It.IsAny<CategoryResponse>());

        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new KeyNotFoundException($"Category with id {id} not found"));

        // Act
        Func<Task> act = async () => await _service.GetCategoryByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Category with id {id} not found");
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task CreateCategoryAsync_Should_Create_Category()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category() { Id = id, Name = "Test", IsActive = true };
        _mapperMock.Setup(x => x.Map<Category>(It.IsAny<CategoryRequest>())).Returns(category);

        _categoryRepoMock
            .Setup<Task>(x => x.CreateAsync(It.IsAny<Category>()));

        // Act
        await _service.CreateCategoryAsync(It.IsAny<CategoryRequest>());

        // Assert
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task CreateCategoryAsync_Should_Throw_When_Category_Null()
    {
        // Arrange
        _mapperMock.Setup(x => x.Map<Category>(It.IsAny<CategoryRequest>())).Returns(It.IsAny<Category>());

        // Act
        Func<Task> act = async () => await _service.CreateCategoryAsync(It.IsAny<CategoryRequest>());

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }
    
    [Fact]
    public async Task CreateCategoryAsync_Should_Throw_When_Category_With_That_Name_Already_Exists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category() { Id = id, Name = "Test", IsActive = true };
        _mapperMock.Setup(x => x.Map<Category>(It.IsAny<CategoryRequest>())).Returns(category);

        _categoryRepoMock.Setup(x => x.AnyAsync(
            It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>()
        )).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _service.CreateCategoryAsync(It.IsAny<CategoryRequest>());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("A category with that name already exists!");
    }
    
    [Fact]
    public async Task UpdateCategoryAsync_Should_Throw_When_Category_With_That_Name_Already_Exists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category() { Id = id, Name = "Test", IsActive = true };

        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(id)).ReturnsAsync(category);
        _categoryRepoMock
            .Setup<Task>(x => x.UpdateAsync(It.IsAny<Category>()));
        _mapperMock.Setup(x => x.Map(It.IsAny<CategoryRequest>(), category)).Returns(category);
        
        _categoryRepoMock.Setup(x => x.AnyAsync(
            It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>()
        )).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _service.UpdateCategoryAsync(id, It.IsAny<CategoryRequest>());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("A category with that name already exists!");
    }
    
    [Fact]
    public async Task UpdateCategoryAsync_Should_Throw_When_Category_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(id)).ReturnsAsync(It.IsAny<Category>());

        // Act
        Func<Task> act = async () => await _service.UpdateCategoryAsync(id, It.IsAny<CategoryRequest>());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Category with id {id} not found");
    }
    
    [Fact]
    public async Task UpdateCategoryAsync_Should_Update_Category()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category() { Id = id, Name = "Test", IsActive = true };

        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(id)).ReturnsAsync(category);
        _categoryRepoMock
            .Setup<Task>(x => x.UpdateAsync(It.IsAny<Category>()));
        _mapperMock.Setup(x => x.Map(It.IsAny<CategoryRequest>(), category)).Returns(category);

        // Act
        await _service.UpdateCategoryAsync(id, It.IsAny<CategoryRequest>());

        // Assert
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_Should_Delete_Category()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category() { Id = id, Name = "Test", IsActive = true };

        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(id)).ReturnsAsync(category);
        _categoryRepoMock.Setup<Task<bool>>(x => x.AnyAsync(
            It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>())).ReturnsAsync(false);
        _categoryRepoMock
            .Setup<Task>(x => x.UpdateAsync(category));

        // Act
        await _service.DeleteCategoryAsync(id);

        // Assert
        _categoryRepoMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_Should_Throw_When_Category_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(id)).ReturnsAsync(It.IsAny<Category>());

        // Act
        Func<Task> act = async () => await _service.DeleteCategoryAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Category with id {id} not found");
    }
    
    [Fact]
    public async Task DeleteCategoryAsync_Should_Throw_When_Category_Has_Active_Products()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category() { Id = id, Name = "Test", IsActive = true };
        _categoryRepoMock
            .Setup<Task<Category?>>(x => x.GetByIdAsync(id)).ReturnsAsync(category);
        _categoryRepoMock.Setup<Task<bool>>(x => x.AnyAsync(
            It.IsAny<IEnumerable<Expression<Func<Category, bool>>>>())).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _service.DeleteCategoryAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("You can not delete a category with active products!");
    }
}