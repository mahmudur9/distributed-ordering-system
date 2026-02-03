using PaymentService.Domain.IRepositories;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DBContext _context;
    public IPaymentRepository PaymentRepository { get; private set; }
    public IPaymentTypeRepository PaymentTypeRepository { get; private set; }
    public IPaymentMethodRepository PaymentMethodRepository { get; private set; }

    public UnitOfWork(DBContext context)
    {
        _context = context;
        PaymentRepository = new PaymentRepository(_context);
        PaymentTypeRepository = new PaymentTypeRepository(_context);
        PaymentMethodRepository = new PaymentMethodRepository(_context);
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