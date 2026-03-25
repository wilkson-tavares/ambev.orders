using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Data.Context;

public sealed class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(p => p.Id);

            builder.HasIndex(p => p.OrderId).IsUnique();

            builder.Property(p => p.OrderId).IsRequired();
            builder.Property(p => p.ClientId).IsRequired();
            builder.Property(p => p.Status).IsRequired();
            builder.Property(p => p.Tax).IsRequired();

            builder.HasMany(p => p.Items)
                   .WithOne()
                   .IsRequired();
        });

        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.HasKey(i => new { i.ProductId });
            builder.Property(i => i.Value).IsRequired();
            builder.Property(i => i.Quantity).IsRequired();
        });
    }
}