using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Infrastructure.Cache;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.BackgroundServices;

public class BackgroundWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BackgroundWorkerService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Migration.Migrate(_serviceScopeFactory, stoppingToken);
        await RedisIndex.CreateAsync(_serviceScopeFactory);
    }
}