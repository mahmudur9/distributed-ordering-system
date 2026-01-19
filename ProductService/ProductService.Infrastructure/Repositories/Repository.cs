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
        Expression<Func<T, object>> include)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        query = query.Include(include).AsNoTracking();
        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates,
        Expression<Func<T, object>> include)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        query = query.Include(include).AsNoTracking();
        return await query.ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates, int itemsPerPage,
        int pageNumber)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        query = query.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);
        return await query.ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(IEnumerable<Expression<Func<T, bool>>> predicates,
        Expression<Func<T, Object>> include, int itemsPerPage, int pageNumber)
    {
        IQueryable<T> query = _context.Set<T>();
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        
        query = query.Include(include).Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).AsNoTracking();
        
        return await query.ToListAsync();
    }
}