using OrderService.Domain.IRepositories;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DBContext _context;
    public IOrderRepository OrderRepository { get; private set; }
    
    public UnitOfWork(DBContext context)
    {
        _context = context;
        OrderRepository = new OrderRepository(_context);
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