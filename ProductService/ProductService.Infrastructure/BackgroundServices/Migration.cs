using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.BackgroundServices;

public class Migration : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Migration(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DBContext>();
            db.Database.Migrate();
        }, stoppingToken);
    }
}