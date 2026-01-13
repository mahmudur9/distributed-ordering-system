using PaymentService.Domain.Models;

namespace PaymentService.Domain.IRepositories;

public interface IPaymentTypeRepository : IRepository<PaymentType>
{
    Task<IEnumerable<PaymentType>> GetAllPaymentTypesAsync(string? name, bool isActive, int itemsPerPage, int pageNumber);
    Task<int> GetAllPaymentTypeCountAsync(string? name, bool isActive);
    Task<bool> PaymentTypeHasAnyPaymentMethodAsync(Guid paymentTypeId);
    Task<bool> PaymentTypeMatchesWithPaymentMethodAsync(Guid paymentTypeId, Guid paymentMethodId);
}