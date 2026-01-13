using ProductService.Domain.IRepositories;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DBContext _context;
    public IProductRepository ProductRepository { get; private set; }
    public ICategoryRepository CategoryRepository { get; private set; }

    public UnitOfWork(DBContext context)
    {
        _context = context;
        ProductRepository = new ProductRepository(_context);
        CategoryRepository = new CategoryRepository(_context);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }
}