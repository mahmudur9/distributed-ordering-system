using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.IRepositories;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

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
}