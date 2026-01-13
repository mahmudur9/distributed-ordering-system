using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.IServices;
using ProductService.Application.Services;

namespace ProductService.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
        
        // Register services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, Services.ProductService>();

        return services;
    }
}