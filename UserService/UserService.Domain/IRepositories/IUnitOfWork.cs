namespace UserService.Domain.IRepositories;

public interface IUnitOfWork
{
    IRoleRepository RoleRepository { get; }
    IUserRepository UserRepository { get; }
    
    Task SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}