using System.Linq.Expressions;
using AutoMapper;
using Moq;
using UserService.Application.Abstractions.Logging;
using UserService.Application.Requests;
using UserService.Application.Responses;
using UserService.Application.Services;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;

namespace UserService.UnitTests;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository>  _roleRepositoryMock = new();
    private readonly Mock<IUnitOfWork>  _unitOfWorkMock = new();
    private readonly Mock<IMapper>  _mapperMock = new();
    private readonly Mock<IAppLogger<RoleService>>  _loggerMock = new();
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.RoleRepository).Returns(_roleRepositoryMock.Object);
        _roleService = new RoleService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task GetAllRolesAsync_Should_Return_Paginated_Response()
    {
        // Arrange
        var filter = new GetAllRolesFilter()
        {
            Name = "Test",
            IsActive = true,
            PageNumber = 1,
            ItemsPerPage = 10
        };

        var roleResponses = new List<RoleResponse>()
        {
            new()
            {
                Id = Guid.Empty,
                Name = "Test"
            }
        };
        
        _mapperMock.Setup(x => x.Map<IEnumerable<RoleResponse>>(It.IsAny<List<Role>>())).Returns(roleResponses);

        _roleRepositoryMock
            .Setup<Task<List<Role>>>(x => x.GetAllAsync(
                It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>?>()
            )).ReturnsAsync(It.IsAny<List<Role>>());

        _roleRepositoryMock
            .Setup<Task<int>>(x => x.CountAsync(
                It.IsAny<IEnumerable<Expression<Func<Role, bool>>>>()
            )).ReturnsAsync(1);

        

        // Act
        var result = await _roleService.GetAllRolesAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Test", result.Data.First().Name);
        _roleRepositoryMock.VerifyAll();
    }
}