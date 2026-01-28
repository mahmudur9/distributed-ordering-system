using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain.ICache;
using ProductService.Domain.IRepositories;
using ProductService.Infrastructure.BackgroundServices;
using ProductService.Infrastructure.Cache;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using StackExchange.Redis;

namespace ProductService.Infrastructure.Extensions;

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

        services.AddHttpClient("object-store-service", client =>
        {
            client.BaseAddress = new Uri(configuration.GetRequiredSection("Services").GetValue<string>("ObjectStoreService")! + "/api/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        
        // Register Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConfig = ConfigurationOptions.Parse(configuration.GetRequiredSection("Services").GetValue<string>("RedisService")!,
                true);

            redisConfig.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(redisConfig);
        });

        services.AddScoped<ICache, RedisCache>();
        
        return services;
    }
}