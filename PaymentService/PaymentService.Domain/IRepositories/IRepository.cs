using System.Linq.Expressions;

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
    Task<int>  CountAsync(Expression<Func<T, bool>>[] predicates);
    Task<bool> AnyAsync(Expression<Func<T, bool>>[] predicates);
    Task<T> GetAsync(Expression<Func<T, bool>>[] predicates);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>[] predicates);
}