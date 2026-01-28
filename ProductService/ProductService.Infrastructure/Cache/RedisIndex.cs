using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ProductService.Infrastructure.Cache;

public static class RedisIndex
{
    public static async Task CreateAsync(IServiceScopeFactory serviceScopeFactory)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            
            var result = await redis.GetDatabase().ExecuteAsync("JSON.GET", "idx:products");
            if (!result.IsNull) return;

            await redis.GetDatabase().ExecuteAsync(
                "FT.CREATE",
                "idx:products",
                "ON", "JSON",
                "PREFIX", "1", "product:",
                "SCHEMA",
                "$.Id", "AS", "Id", "TAG",
                "$.Name", "AS", "Name", "TEXT",
                "$.Description", "AS", "Description", "TEXT",
                "$.SellingPrice", "AS", "SellingPrice", "NUMERIC",
                "$.Stock", "AS", "Stock", "NUMERIC",
                "$.IsActive", "AS", "IsActive", "TAG",
                "$.CreatedAt", "AS", "CreatedAt", "TEXT",
                "$.UpdatedAt", "AS", "UpdatedAt", "TEXT",
                "$.Category.Name", "AS", "CategoryName", "TEXT",
                "$.Pictures[*].Url", "AS", "Url", "TEXT"
            );

            var json = @"{""Name"":""RedisIndex""}";
            await redis.GetDatabase().ExecuteAsync("JSON.SET", "idx:products", "$", json);
        }
    }
}