using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain.IRepositories;
using ProductService.Infrastructure.BackgroundServices;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;

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
        
        return services;
    }
}