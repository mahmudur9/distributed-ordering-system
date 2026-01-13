namespace PaymentService.Domain.IRepositories;

public interface IRepository<T> where T : class
{
    Task CreateAsync(T model);
    Task CreateRangeAsync(IEnumerable<T> models);
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task UpdateAsync(T model);
    Task UpdateRangeAsync(IEnumerable<T> models);
    Task DeleteAsync(T model);
    Task DeleteRangeAsync(IEnumerable<T> models);
}