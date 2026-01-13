using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
{
    public PaymentMethodRepository(DBContext context) : base(context)
    {
    }
    
    private IQueryable<PaymentMethod> SearchByName(IQueryable<PaymentMethod> query, string name)
    {
        return query.Where(x => (x.Name).ToLower().Contains(name.ToLower()));
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllPaymentMethodsAsync(string? name, bool isActive, int itemsPerPage,
        int pageNumber)
    {
        var paymentMethods = _context.PaymentMethods.Where(x => x.IsActive == isActive).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            paymentMethods = SearchByName(paymentMethods, name);
        }

        paymentMethods = paymentMethods.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

        return await paymentMethods.ToListAsync();
    }

    public async Task<int> GetAllPaymentMethodCountAsync(string? name, bool isActive)
    {
        var paymentMethods = _context.PaymentMethods.Where(x => x.IsActive == isActive).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            paymentMethods = SearchByName(paymentMethods, name);
        }

        return await paymentMethods.CountAsync();
    }
}