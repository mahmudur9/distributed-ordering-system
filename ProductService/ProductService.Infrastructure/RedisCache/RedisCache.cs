using System.Text.Json;
using ProductService.Domain.ICache;
using StackExchange.Redis;

namespace ProductService.Infrastructure.RedisCache;

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

    public async Task<T?> GetAsync<T>(string key)
    {
        var result = await _database.ExecuteAsync("JSON.GET", key);
        string json = result.ToString();
        return JsonSerializer.Deserialize<T>(json);
    }
}