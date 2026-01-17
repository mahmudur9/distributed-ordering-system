using Microsoft.Extensions.DependencyInjection;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.BackgroundServices;

public static class DataSeeding
{
    public static async Task Seed(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (await unitOfWork.RoleRepository.GetAllCountAsync() == 0)
        {
            List<Role> roles = [new Role(){Name = "Admin"}, new Role(){Name = "User"}];
            await unitOfWork.RoleRepository.CreateRangeAsync(roles);
            await unitOfWork.SaveChangesAsync();
        }
    }
}