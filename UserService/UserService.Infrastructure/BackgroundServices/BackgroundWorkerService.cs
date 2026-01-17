using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UserService.Infrastructure.BackgroundServices;

public class BackgroundWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BackgroundWorkerService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DataSeeding.Seed(_serviceScopeFactory);
    }
}