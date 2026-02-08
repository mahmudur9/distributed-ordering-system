using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Domain.ILogging;
using PaymentService.Domain.IRepositories;
using PaymentService.Infrastructure.BackgroundServices;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Logging;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DBContext
        services.AddDbContext<DBContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Register repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register background services
        services.AddHostedService<BackgroundWorkerService>();
        
        // Register httpClient
        services.AddHttpClient("user-service", client =>
        {
            client.BaseAddress = new Uri(configuration.GetRequiredSection("Services").GetValue<string>("UserService")! + "/api/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        
        // Register in-memory
        services.AddMemoryCache();
        
        // Register logging
        services.AddScoped(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));
        
        return services;
    }
}