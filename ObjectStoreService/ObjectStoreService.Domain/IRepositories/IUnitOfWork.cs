namespace ObjectStoreService.Domain.IRepositories;

public interface IUnitOfWork
{
    IMediaRepository MediaRepository { get; }
    
    Task SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}