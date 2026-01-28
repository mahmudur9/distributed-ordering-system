namespace ProductService.Domain.ICache;

public interface ICache
{
    Task SetAsync<T>(string key, T value, int? ttl = null);
    Task<T?> GetAsync<T>(string key);
}