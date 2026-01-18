using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.IRepositories;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

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
        await Task.Run(() =>
        {
            _context.Update(model);
        });
    }

    public async Task UpdateRangeAsync(IEnumerable<T> models)
    {
        await Task.Run(() =>
        {
            _context.UpdateRange(models);
        });
    }

    public async Task DeleteAsync(T model)
    {
        await Task.Run(() =>
        {
            _context.Set<T>().Remove(model);
        });
    }

    public async Task DeleteRangeAsync(IEnumerable<T> models)
    {
        await Task.Run(() =>
        {
            _context.Set<T>().RemoveRange(models);
        });
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>[] predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        return query.CountAsync();
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>>[] predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        return query.AnyAsync();
    }

    public Task<T> GetAsync(Expression<Func<T, bool>>[] predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        return query.FirstOrDefaultAsync()!;
    }

    public Task<List<T>> GetAllAsync(Expression<Func<T, bool>>[] predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        return query.ToListAsync();
    }
}