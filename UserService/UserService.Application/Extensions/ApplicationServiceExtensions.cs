using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.IServices;
using UserService.Application.Services;

namespace UserService.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
        
        // Register services
        // services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, Services.UserService>();

        return services;
    }
}