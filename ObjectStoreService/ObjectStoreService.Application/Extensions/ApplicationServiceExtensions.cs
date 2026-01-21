using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObjectStoreService.Application.IServices;
using ObjectStoreService.Application.Services;

namespace ObjectStoreService.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
        
        // Register services
        services.AddScoped<IMediaService, MediaService>();
        
        // Register http context accessor
        services.AddHttpContextAccessor();
        
        return services;
    }
}