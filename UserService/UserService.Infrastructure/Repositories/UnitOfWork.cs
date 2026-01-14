using UserService.Domain.IRepositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DBContext _context;
    public IRoleRepository RoleRepository { get; private set; }
    public IUserRepository UserRepository { get; private set; }

    public UnitOfWork(DBContext context)
    {
        _context = context;
        RoleRepository = new RoleRepository(_context);
        UserRepository = new UserRepository(_context);
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