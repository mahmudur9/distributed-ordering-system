using PaymentService.Domain.Models;

namespace PaymentService.Domain.IRepositories;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    Task<IEnumerable<PaymentMethod>> GetAllPaymentMethodsAsync(string? name, bool isActive, int itemsPerPage, int pageNumber);
    Task<int> GetAllPaymentMethodCountAsync(string? name, bool isActive);
}