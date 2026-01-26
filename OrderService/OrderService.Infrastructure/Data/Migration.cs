using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Infrastructure.Data;

public static class Migration
{
    public static async Task Migrate(IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DBContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}