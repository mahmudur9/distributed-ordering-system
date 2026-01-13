using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Models;

namespace OrderService.Infrastructure.Data;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<ProductOrder> ProductOrders { get; set; }
}