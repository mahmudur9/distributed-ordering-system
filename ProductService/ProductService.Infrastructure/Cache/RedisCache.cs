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
    
    public async Task SetJsonAsync<T>(string key, T value, int? ttl = null)
    {
        await _database.ExecuteAsync("JSON.SET", key, "$", JsonSerializer.Serialize(value));
        if (ttl is not null)
        {
            await _database.ExecuteAsync("EXPIRE", key, "EX", ttl); // Time to live in second
        }
    }

    public async Task SetJsonAsync<T>(string key, string path, T value, int? ttl = null)
    {
        await _database.ExecuteAsync("JSON.SET", key, "$." + path, value!);
        if (ttl is not null)
        {
            await _database.ExecuteAsync("EXPIRE", key, "EX", ttl); // Time to live in second
        }
    }

    public async Task<string?> GetJsonAsync(string key)
    {
        var result = await _database.ExecuteAsync("JSON.GET", key);
        if (result.IsNull)
        {
            return null;
        }

        return result.ToString();
    }

    public async Task<IEnumerable<T>> GetAllJsonAsync<T>(string index, string query, int pageNumber, int itemsPerPage)
    {
        pageNumber = (pageNumber - 1) * itemsPerPage;
        if (pageNumber < 0) pageNumber = 0;
        
        var result = await _database.ExecuteAsync("FT.SEARCH", index, query, "LIMIT", $"{pageNumber}", $"{itemsPerPage}");
        if (result.IsNull)
        {
            return [];
        }

        var redisResult = (RedisResult[])result!;
        
        var list = new List<T>();

        for (int i = 1; i < redisResult.Length; i += 2)
        {
            var fields = (RedisResult[])redisResult[i + 1]!;

            for (int j = 0; j < fields.Length; j += 2)
            {
                var json = fields[j + 1].ToString();
                list.Add(JsonSerializer.Deserialize<T>(json)!);
            }
        }

        return list;
    }

    public async Task<IEnumerable<T>> GetAllJsonAsync<T>(string index, string query, int pageNumber, int itemsPerPage,
        string orderByField, string orderByDirection) // orderByDirection DESC | ASC
    {
        pageNumber = (pageNumber - 1) * itemsPerPage;
        if (pageNumber < 0) pageNumber = 0;
        
        var result = await _database.ExecuteAsync(
            "FT.SEARCH", 
            index, query,
            "SORTBY", $"{orderByField}", $"{orderByDirection}",
            "LIMIT", $"{pageNumber}", $"{itemsPerPage}");
        if (result.IsNull)
        {
            return [];
        }

        var redisResult = (RedisResult[])result!;
        
        var list = new List<T>();

        for (int i = 1; i < redisResult.Length; i += 2)
        {
            var fields = (RedisResult[])redisResult[i + 1]!;

            for (int j = 0; j < fields.Length; j += 4)
            {
                var json = fields[j + 3].ToString();
                list.Add(JsonSerializer.Deserialize<T>(json)!);
            }
        }

        return list;
    }

    public async Task<int> GetAllCountAsync(string index, string query)
    {
        var result = await _database.ExecuteAsync("FT.SEARCH", index, query, "LIMIT", "0", "0");
        
        var arr = (RedisResult[])result!;
        int totalCount = (int)arr[0];

        return totalCount;
    }

    public async Task DeleteJsonAsync(string key)
    {
        await _database.ExecuteAsync("JSON.DEL", key);
    }
}