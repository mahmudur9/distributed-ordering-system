using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(DBContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(bool isActive, DateTime? dateFrom, DateTime? dateTo,
        Guid? paymentId, int itemsPerPage, int pageNumber)
    {
        var payments = _context.Payments.Where(x => x.IsActive == isActive)
            .Include(x => x.PaymentMethod)
            .Include(x => x.PaymentType).AsQueryable();

        if (dateFrom is not null)
        {
            payments.Where(x => x.CreatedAt >= dateFrom).AsQueryable();
        }

        if (dateTo is not null)
        {
            payments.Where(x => x.CreatedAt <= dateTo).AsQueryable();
        }

        if (paymentId is not null)
        {
            payments.Where(x => x.Id == paymentId).AsQueryable();
        }

        payments = payments.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

        return await payments.ToListAsync();
    }

    public async Task<int> GetAllPaymentCountAsync(bool isActive, DateTime? dateFrom, DateTime? dateTo, Guid? paymentId)
    {
        var payments = _context.Payments.Where(x => x.IsActive == isActive).AsQueryable();

        if (dateFrom is not null)
        {
            payments.Where(x => x.CreatedAt >= dateFrom).AsQueryable();
        }

        if (dateTo is not null)
        {
            payments.Where(x => x.CreatedAt <= dateTo).AsQueryable();
        }

        if (paymentId is not null)
        {
            payments.Where(x => x.Id == paymentId).AsQueryable();
        }

        return await payments.CountAsync();
    }
}