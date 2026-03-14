using Backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ScanEvent> ScanEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Inventory>()
            .HasKey(i => new { i.ProductId, i.LocationId });

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId);

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Location)
            .WithMany()
            .HasForeignKey(i => i.LocationId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);
    }
}
