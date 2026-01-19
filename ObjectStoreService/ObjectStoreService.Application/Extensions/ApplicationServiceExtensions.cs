using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ObjectStoreService.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
        
        // Register services
        
        
        return services;
    }
}