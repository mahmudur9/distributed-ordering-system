using PaymentService.Domain.Models;

namespace PaymentService.Domain.IRepositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetAllPaymentsAsync(bool isActive, DateTime? dateFrom, DateTime? dateTo,
        Guid? paymentId, int itemsPerPage, int pageNumber);
    Task<int> GetAllPaymentCountAsync(bool isActive, DateTime? dateFrom, DateTime? dateTo, Guid? paymentId);
}