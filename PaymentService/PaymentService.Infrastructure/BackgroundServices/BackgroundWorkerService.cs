using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.BackgroundServices;

public class BackgroundWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BackgroundWorkerService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Migration.MigrateAsync(_serviceScopeFactory, stoppingToken);
        await DataSeeding.SeedAsync(_serviceScopeFactory);
    }
}