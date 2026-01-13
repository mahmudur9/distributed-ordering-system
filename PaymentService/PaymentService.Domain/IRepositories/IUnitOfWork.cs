namespace PaymentService.Domain.IRepositories;

public interface IUnitOfWork
{
    IPaymentRepository PaymentRepository { get; }
    IPaymentTypeRepository PaymentTypeRepository { get; }
    IPaymentMethodRepository PaymentMethodRepository { get; }
    
    Task SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}