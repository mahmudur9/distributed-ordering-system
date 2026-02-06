using AutoMapper;
using FluentAssertions;
using Moq;
using UserService.Application.Abstractions.Logging;
using UserService.Application.Abstractions.Security;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;

namespace UserService.UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository>  _userRepositoryMock = new();
    private readonly Mock<IRoleRepository>  _roleRepositoryMock = new();
    private readonly Mock<IUnitOfWork>  _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher>   _passwordHasherMock = new();
    private readonly Mock<IMapper>  _mapperMock = new();
    private readonly Mock<IAppLogger<Application.Services.UserService>>  _loggerMock = new();
    private readonly Mock<IAuthService>   _authServiceMock = new();
    private readonly Application.Services.UserService  _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.RoleRepository).Returns(_roleRepositoryMock.Object);
        _userService = new Application.Services.UserService( _unitOfWorkMock.Object, _mapperMock.Object, _authServiceMock.Object, _loggerMock.Object, _passwordHasherMock.Object);
    }
    
    [Fact]
    public async Task GetAllUsersAsync_Should_Return_Paginated_Response()
    {
        // Arrange
        var filter = new GetAllUsersFilter()
        {
            Name = "Test",
            IsActive = true,
            PageNumber = 1,
            ItemsPerPage = 10
        };

        var userResponses = new List<UserResponse>()
        {
            new()
            {
                Id = Guid.Empty,
                Name = "Test",
                Email = "Test",
                Phone = "7847834"
            }
        };
        
        _mapperMock.Setup(x => x.Map<IEnumerable<UserResponse>>(It.IsAny<List<User>>())).Returns(userResponses);

        _userRepositoryMock
            .Setup<Task<List<User>>>(x => x.GetAllUsersAsync(
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(It.IsAny<List<User>>());

        _userRepositoryMock
            .Setup<Task<int>>(x => x.GetAllUserCountAsync(
                It.IsAny<string?>(),
                It.IsAny<bool>()
            )).ReturnsAsync(1);

        

        // Act
        var result = await _userService.GetAllUsersAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Test", result.Data.First().Name);
        _userRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task CreateUserAsync_Should_Create_User()
    {
        // Arrange
        var userRequest = new UserRequest()
        {
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty
        };
        var user = new User()
        {
            Id = Guid.Empty,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty
        };
        var role = new Role()
        {
            Id = Guid.Empty,
            Name = "Test",
        };
        _roleRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(role);
        _passwordHasherMock.Setup(x => x.GenerateHash(user.Password)).Returns(user.Password);
        _mapperMock.Setup(x => x.Map<User>(userRequest)).Returns(user);
        _userRepositoryMock.Setup(x => x.CreateAsync(user))
            .Returns(Task.CompletedTask);
        
        // Act
        await _userService.CreateUserAsync(userRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _userRepositoryMock.VerifyAll();
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task CreateUserAsync_Should_Throw_When_Role_Not_Found()
    {
        // Arrange
        var userRequest = new UserRequest()
        {
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty
        };
       
        _roleRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new KeyNotFoundException("Role not found"));
        
        // Act
        Func<Task> act = async () => await _userService.CreateUserAsync(userRequest);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Role not found");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdateUserAsync_Should_Update_User()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userRequest = new UpdateUserRequest()
        {
            Name = "Test",
            Email = "Test",
            Phone = "7847834"
        };
        var user = new User()
        {
            Id = id,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty
        };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(user);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(id);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserRole()).Returns("Admin");
        _mapperMock.Setup(x => x.Map(userRequest, user)).Returns(user);
        _userRepositoryMock.Setup(x => x.UpdateAsync(user))
            .Returns(Task.CompletedTask);
        
        // Act
        await _userService.UpdateUserAsync(id, userRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _userRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdateUserAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userRequest = new UpdateUserRequest()
        {
            Name = "Test",
            Email = "Test",
            Phone = "7847834"
        };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException("User not found"));
        
        // Act
        Func<Task> act = async () => await _userService.UpdateUserAsync(id, userRequest);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdateUserAsync_Should_Throw_When_User_Not_Admin_And_Want_To_Update_Another_User()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userRequest = new UpdateUserRequest()
        {
            Name = "Test",
            Email = "Test",
            Phone = "7847834"
        };
        var user = new User()
        {
            Id = id,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty
        };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(user);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(Guid.NewGuid());
        _authServiceMock.Setup(x => x.GetAuthenticatedUserRole())
            .Returns("User");
        // Act
        Func<Task> act = async () => await _userService.UpdateUserAsync(id, userRequest);
        
        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Unauthorized");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeleteUserAsync_Should_Delete_User()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User()
        {
            Id = id,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty
        };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(user);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserRole()).Returns("Admin");
        _userRepositoryMock.Setup(x => x.UpdateAsync(user))
            .Returns(Task.CompletedTask);
        
        // Act
        await _userService.DeleteUserAsync(id);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _userRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeleteUserAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException("User not found"));
        
        // Act
        Func<Task> act = async () => await _userService.DeleteUserAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task DeleteUserAsync_Should_Throw_When_User_Not_Admin()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User()
        {
            Id = id,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty
        };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(user);
        _authServiceMock.Setup(x => x.GetAuthenticatedUserRole()).Returns("User");
        
        // Act
        Func<Task> act = async () => await _userService.DeleteUserAsync(id);
        
        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Unauthorized");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePasswordAsync_Should_Update_Password()
    {
        // Arrange
        var id = Guid.NewGuid();
        var passwordRequest = new UpdatePasswordRequest()
        {
            CurrentPassword = "123456",
            NewPassword = "123456"
        };
        var user = new User()
        {
            Id = id,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "123456",
            RoleId = Guid.Empty
        };
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(id);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPasswordHash(
            It.IsAny<string>(), It.IsAny<string>()
        )).Returns(true);
        _passwordHasherMock.Setup(x => x.GenerateHash(user.Password)).Returns(It.IsAny<string>());
        _userRepositoryMock.Setup(x => x.UpdateAsync(user))
            .Returns(Task.CompletedTask);
        
        // Act
        await _userService.UpdatePasswordAsync(id, passwordRequest);
        
        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _userRepositoryMock.VerifyAll();
        _passwordHasherMock.VerifyAll();
        _authServiceMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePasswordAsync_Should_Throw_When_User_Not_Authenticated()
    {
        // Arrange
        var id = Guid.NewGuid();
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(Guid.Empty);
        
        // Act
        Func<Task> act = async () => await _userService.UpdatePasswordAsync(id, It.IsAny<UpdatePasswordRequest>());
        
        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Unauthorized");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePasswordAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(id);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException("User not found"));
        
        // Act
        Func<Task> act = async () => await _userService.UpdatePasswordAsync(id, It.IsAny<UpdatePasswordRequest>());
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task UpdatePasswordAsync_Should_Throw_When_CurrentPassword_Incorrect()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User()
        {
            Id = id,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "123456",
            RoleId = Guid.Empty
        };
        var passwordRequest = new UpdatePasswordRequest()
        {
            CurrentPassword = "123456",
            NewPassword = "123456"
        };
        _authServiceMock.Setup(x => x.GetAuthenticatedUserId()).Returns(id);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPasswordHash(
            It.IsAny<string>(), It.IsAny<string>()
        )).Returns(false);
        
        // Act
        Func<Task> act = async () => await _userService.UpdatePasswordAsync(id, passwordRequest);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Incorrect current password!");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task LoginAsync_Should_Login()
    {
        // Arrange
        var loginRequest = new LoginRequest()
        {
            Email = "test@gmail.com",
            Password = "123456"
        };
        var user = new User()
        {
            Id = Guid.Empty,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty,
            Role = new()
            {
                Id = Guid.Empty,
                Name = "User"
            }
        };
        
        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _authServiceMock.Setup(x => x.GenerateToken(user.Id, user.Email, user.Role.Name))
            .Returns("test");
        
        // Act
        var result = await _userService.LoginAsync(loginRequest);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result.Token);
        _userRepositoryMock.VerifyAll();
        _passwordHasherMock.VerifyAll();
        _authServiceMock.VerifyAll();
    }
    
    [Fact]
    public async Task LoginAsync_Should_Throw_When_Incorrect_Email()
    {
        // Arrange
        var loginRequest = new LoginRequest()
        {
            Email = "test@gmail.com",
            Password = "123456"
        };
        
        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<User>());
        
        // Act
        Func<Task> act = async () => await _userService.LoginAsync(loginRequest);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Incorrect email or password!");
        _roleRepositoryMock.VerifyAll();
    }
    
    [Fact]
    public async Task LoginAsync_Should_Throw_When_Incorrect_Password()
    {
        // Arrange
        var loginRequest = new LoginRequest()
        {
            Email = "test@gmail.com",
            Password = "123456"
        };
        var user = new User()
        {
            Id = Guid.Empty,
            Name = "Test",
            Email = "Test",
            Phone = "7847834",
            Password = "8434348",
            RoleId = Guid.Empty,
            Role = new()
            {
                Id = Guid.Empty,
                Name = "User"
            }
        };
        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        
        // Act
        Func<Task> act = async () => await _userService.LoginAsync(loginRequest);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Incorrect email or password!");
        _roleRepositoryMock.VerifyAll();
    }
}