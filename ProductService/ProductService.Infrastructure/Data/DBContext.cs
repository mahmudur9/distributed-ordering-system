using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;

namespace ProductService.Infrastructure.Data;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}