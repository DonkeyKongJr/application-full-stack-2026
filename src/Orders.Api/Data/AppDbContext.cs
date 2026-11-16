using Microsoft.EntityFrameworkCore;
using Orders.Api.Abstractions;
using Orders.Api.Models;

namespace Orders.Api.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).HasMaxLength(32).IsRequired();
            entity.HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId);
            entity.HasMany(o => o.Lines).WithOne(l => l.Order).HasForeignKey(l => l.OrderId);

            // Tenant + soft-delete isolation for all regular queries
            entity.HasQueryFilter(o =>
                !o.IsDeleted && o.CustomerId == _currentUser.CustomerId);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(200).IsRequired();
        });
    }
}
