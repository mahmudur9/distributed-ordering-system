using Microsoft.Extensions.DependencyInjection;
using PaymentService.Domain.IRepositories;
using PaymentService.Domain.Models;

namespace PaymentService.Infrastructure.Data;

public static class DataSeeding
{
    public static async Task SeedAsync(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (!await unitOfWork.PaymentTypeRepository.AnyAsync([x => x.IsActive]))
        {
            var paymentType = new PaymentType()
            {
                Name = "Mobile Banking"
            };
            var paymentTypes = new List<PaymentType>()
            {
                paymentType,
                new PaymentType()
                {
                    Name = "Cash On Delivery"
                }
            };
            await unitOfWork.PaymentTypeRepository.CreateRangeAsync(paymentTypes);
            await unitOfWork.SaveChangesAsync();

            var paymentMethods = new List<PaymentMethod>()
            {
                new PaymentMethod()
                {
                    Name = "Bkash",
                    PaymentTypeId = paymentType.Id
                },
                new PaymentMethod()
                {
                    Name = "Nagad",
                    PaymentTypeId = paymentType.Id
                }
            };
            await unitOfWork.PaymentMethodRepository.CreateRangeAsync(paymentMethods);
            await unitOfWork.SaveChangesAsync();
        }
    }
}