using Billing.Models;
using Microsoft.EntityFrameworkCore;

namespace Billing.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
    }

    public DbSet<BillingAccount> BillingAccounts => Set<BillingAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingAccount>(entity =>
        {
            entity.ToTable("BillingAccounts", "billing");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.OpenBalance).HasPrecision(18, 2);
            entity.Property(a => a.Currency).HasMaxLength(3).IsRequired();
            entity.HasIndex(a => a.CustomerId).IsUnique();
        });
    }
}
