using System.Linq.Expressions;

namespace UserService.Domain.IRepositories;

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
    Task<int> GetAllCountAsync();
    Task<int>  CountAsync(IEnumerable<Expression<Func<T, bool>>> predicates);
    Task<bool> AnyAsync(IEnumerable<Expression<Func<T, bool>>> predicates);
    Task<T?> GetAsync(IEnumerable<Expression<Func<T, bool>>> predicates);
    Task<T?> GetAsync(IEnumerable<Expression<Func<T, bool>>> predicates, Expression<Func<T, Object>> include);
    Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates, Expression<Func<T, Object>> include, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates, int itemsPerPage, int pageNumber, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates, Expression<Func<T, Object>> include, int itemsPerPage, int pageNumber, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
}