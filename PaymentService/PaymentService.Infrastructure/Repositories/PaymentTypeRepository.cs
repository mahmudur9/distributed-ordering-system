using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentTypeRepository : Repository<PaymentType>, IPaymentTypeRepository
{
    public PaymentTypeRepository(DBContext context) : base(context)
    {
    }
    
    private IQueryable<PaymentType> SearchByName(IQueryable<PaymentType> query, string name)
    {
        return query.Where(x => (x.Name).ToLower().Contains(name.ToLower()));
    }

    public async Task<IEnumerable<PaymentType>> GetAllPaymentTypesAsync(string? name, bool isActive, int itemsPerPage,
        int pageNumber)
    {
        var paymentTypes = _context.PaymentTypes.Where(x => x.IsActive == isActive).Include(x => x.PaymentMethods).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            paymentTypes = SearchByName(paymentTypes, name);
        }

        paymentTypes = paymentTypes.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

        return await paymentTypes.AsNoTracking().ToListAsync();
    }

    public async Task<int> GetAllPaymentTypeCountAsync(string? name, bool isActive)
    {
        var paymentTypes = _context.PaymentTypes.Where(x => x.IsActive == isActive).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            paymentTypes = SearchByName(paymentTypes, name);
        }

        return await paymentTypes.CountAsync();
    }

    public async Task<bool> PaymentTypeHasAnyPaymentMethodAsync(Guid paymentTypeId)
    {
        return  await _context.PaymentMethods.AnyAsync(x => x.PaymentTypeId == paymentTypeId);
    }

    public async Task<bool> PaymentTypeMatchesWithPaymentMethodAsync(Guid paymentTypeId, Guid paymentMethodId)
    {
        return await _context.PaymentMethods.AnyAsync(x => x.Id == paymentMethodId && x.PaymentTypeId == paymentTypeId);
    }
}