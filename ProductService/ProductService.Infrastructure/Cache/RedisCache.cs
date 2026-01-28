using System.Text.Json;
using ProductService.Domain.ICache;
using StackExchange.Redis;

namespace ProductService.Infrastructure.Cache;

public class RedisCache : ICache
{
    private readonly IDatabase _database;

    public RedisCache(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }
    
    public async Task SetAsync<T>(string key, T value, int? ttl = null)
    {
        await _database.ExecuteAsync("JSON.SET", key, "$", JsonSerializer.Serialize(value));
        if (ttl is not null)
        {
            await _database.ExecuteAsync("EXPIRE", key, "EX", ttl); // Time to live in second
        }
    }

    public async Task<string?> GetAsync<T>(string key)
    {
        var result = await _database.ExecuteAsync("JSON.GET", key);
        if (result.IsNull)
        {
            return null;
        }

        return result.ToString();
    }
}