using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.IServices;
using PaymentService.Application.Services;

namespace PaymentService.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
        
        // Register services
        services.AddScoped<IPaymentTypeService, PaymentTypeService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IPaymentService, Services.PaymentService>();

        return services;
    }
}