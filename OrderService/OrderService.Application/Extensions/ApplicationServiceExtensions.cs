using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.IServices;

namespace OrderService.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
        
        // Register services
        services.AddScoped<IOrderService, Services.OrderService>();

        return services;
    }
}