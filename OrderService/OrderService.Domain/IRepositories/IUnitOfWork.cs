namespace OrderService.Domain.IRepositories;

public interface IUnitOfWork
{
    IOrderRepository OrderRepository { get; }
    
    Task SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}