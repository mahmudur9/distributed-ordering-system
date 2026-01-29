using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Domain.IRepositories;
using OrderService.Infrastructure.BackgroundServices;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using PaymentService.API;
using ProductService.API;

namespace OrderService.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DBContext
        services.AddDbContext<DBContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register gRPC clients
        services.AddGrpcClient<ProductGrpcService.ProductGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration.GetRequiredSection("Services").GetValue<string>("ProductService")!);
        });
        services.AddGrpcClient<PaymentGrpcService.PaymentGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration.GetRequiredSection("Services").GetValue<string>("PaymentService")!);
        });

        // Register background services
        services.AddHostedService<BackgroundWorkerService>();
        
        // Register httpClient
        services.AddHttpClient("user-service", client =>
        {
            client.BaseAddress = new Uri(configuration.GetRequiredSection("Services").GetValue<string>("UserService")! + "/api/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        
        services.AddMemoryCache();

        return services;
    }
}