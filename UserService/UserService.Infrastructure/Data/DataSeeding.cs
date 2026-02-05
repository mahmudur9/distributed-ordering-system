using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Abstractions.Security;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Data;

public static class DataSeeding
{
    public static async Task SeedAsync(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var authService = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        
        if (await unitOfWork.RoleRepository.GetAllCountAsync() == 0 && await unitOfWork.UserRepository.GetAllCountAsync() == 0)
        {
            List<Role> roles = [new() { Name = "Admin" }, new() { Name = "User" }];
            await unitOfWork.RoleRepository.CreateRangeAsync(roles);
            await unitOfWork.SaveChangesAsync();
            
            List<User> users =
            [
                new()
                {
                    Name = "Admin",
                    Email = "admin@gmail.com",
                    Password = authService.GenerateHash("123456"),
                    RoleId =  roles[0].Id
                }
            ];
            await unitOfWork.UserRepository.CreateRangeAsync(users);
            await unitOfWork.SaveChangesAsync();
        }
    }
}