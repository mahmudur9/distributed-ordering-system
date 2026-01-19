using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObjectStoreService.Domain.IRepositories;
using ObjectStoreService.Infrastructure.Data;
using ObjectStoreService.Infrastructure.Repositories;

namespace ObjectStoreService.Infrastructure.Extensions;

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
        
        
        // Register background services


        return services;
    }
}