namespace ProductService.Domain.IRepositories;

public interface IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    
    Task SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}