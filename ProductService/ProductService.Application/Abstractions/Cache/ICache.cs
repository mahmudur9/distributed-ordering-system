namespace ProductService.Domain.ICache;

public interface ICache
{
    Task SetJsonAsync<T>(string key, T value, int? ttl = null);
    Task SetJsonAsync<T>(string key, string path, T value, int? ttl = null);
    Task<string?> GetJsonAsync(string key);
    Task<IEnumerable<T>> GetAllJsonAsync<T>(string index, string query, int pageNumber, int itemsPerPage);
    Task<IEnumerable<T>> GetAllJsonAsync<T>(string index, string query, int pageNumber, int itemsPerPage,
        string orderByField, string orderByDirection);
    Task<int> GetAllCountAsync(string index, string query);
    Task DeleteJsonAsync(string key);
}