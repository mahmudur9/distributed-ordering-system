using ObjectStoreService.Domain.IRepositories;
using ObjectStoreService.Infrastructure.Data;

namespace ObjectStoreService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DBContext _context;
    public IMediaRepository MediaRepository { get; private set; }
    
    public UnitOfWork(DBContext context)
    {
        _context = context;
        MediaRepository = new MediaRepository(_context);
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
        await (_context.Database.CurrentTransaction?.RollbackAsync() ?? Task.CompletedTask);
    }
}