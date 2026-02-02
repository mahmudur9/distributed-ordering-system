using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.IServices;
using OrderService.Application.Services;

namespace OrderService.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
        
        // Register services
        services.AddScoped<IOrderService, Services.OrderService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddHttpContextAccessor();

        return services;
    }
}