using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ObjectStoreService.Domain.IRepositories;
using ObjectStoreService.Infrastructure.Data;

namespace ObjectStoreService.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DBContext _context;

    public Repository(DBContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(T model)
    {
        await _context.Set<T>().AddAsync(model);
    }

    public async Task CreateRangeAsync(IEnumerable<T> models)
    {
        await _context.Set<T>().AddRangeAsync(models);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task UpdateAsync(T model)
    {
        await Task.Run(() => { _context.Update(model); });
    }

    public async Task UpdateRangeAsync(IEnumerable<T> models)
    {
        await Task.Run(() => { _context.UpdateRange(models); });
    }

    public async Task DeleteAsync(T model)
    {
        await Task.Run(() => { _context.Set<T>().Remove(model); });
    }

    public async Task DeleteRangeAsync(IEnumerable<T> models)
    {
        await Task.Run(() => { _context.Set<T>().RemoveRange(models); });
    }

    public async Task<int> CountAsync(IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();
    }

    public async Task<bool> AnyAsync(IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        return await query.AnyAsync();
    }

    public async Task<T?> GetAsync(IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        return await query.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<T?> GetAsync(IEnumerable<Expression<Func<T, bool>>> predicates,
        IEnumerable<Expression<Func<T, object>>> includes)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates,
        IEnumerable<Expression<Func<T, object>>> includes, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        query = query.AsNoTracking();
        return await query.ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates, int itemsPerPage,
        int pageNumber, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        query = query.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);
        return await query.ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates,
        IEnumerable<Expression<Func<T, Object>>> includes, int itemsPerPage, int pageNumber,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        query = query.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).AsNoTracking();

        return await query.ToListAsync();
    }
}