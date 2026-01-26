using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Domain.IRepositories;
using UserService.Infrastructure.BackgroundServices;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.Extensions;

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