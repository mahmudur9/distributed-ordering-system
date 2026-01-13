using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Models;

namespace PaymentService.Infrastructure.Data;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<PaymentType> PaymentTypes { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Payment> Payments { get; set; }
}