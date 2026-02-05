using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Abstractions.Logging;
using UserService.Application.Abstractions.Security;
using UserService.Domain.IRepositories;
using UserService.Infrastructure.BackgroundServices;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Logging;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Security;

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
        
        // Register logging
        services.AddScoped(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));
        
        // Register security services
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}