namespace Backend.Tests.Integration.Fixtures;

using Backend.Data;
using Backend.Data.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class WarehouseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbInstanceName;

    public WarehouseWebApplicationFactory()
    {
        // Ensure the test environment is set early so Program.cs can read it before WebApplicationFactory runs
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        // Create unique database name for each factory instance to avoid conflicts
        _dbInstanceName = $"WarehouseTestDb_{Guid.NewGuid()}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext descriptor
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WarehouseDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register with unique in-memory database name
            services.AddDbContext<WarehouseDbContext>(options =>
                options.UseInMemoryDatabase(_dbInstanceName));

            // Build service provider to seed the database
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                SeedTestData(db);
            }
        });
    }

    private static void SeedTestData(WarehouseDbContext context)
    {
        // Seed Products
        var product1 = new Product
        {
            Id = 1,
            Sku = "8872",
            Name = "USB Cable",
            Barcode = "5401234567890"
        };

        var product2 = new Product
        {
            Id = 2,
            Sku = "9001",
            Name = "HDMI Cable",
            Barcode = "5401234567891"
        };

        context.Products.AddRange(product1, product2);

        // Seed Locations
        var location1 = new Location
        {
            Id = 1,
            Aisle = "A",
            Rack = 7,
            Slot = 3,
            X = 10,
            Y = 5,
            Z = 1
        };

        var location2 = new Location
        {
            Id = 2,
            Aisle = "B",
            Rack = 2,
            Slot = 1,
            X = 20,
            Y = 3,
            Z = 2
        };

        context.Locations.AddRange(location1, location2);

        // Seed Inventory (composite key: ProductId, LocationId)
        context.Inventory.AddRange(
            new Inventory { ProductId = 1, LocationId = 1, Quantity = 5 },
            new Inventory { ProductId = 2, LocationId = 2, Quantity = 10 }
        );

        // Seed Orders
        var order1 = new Order
        {
            Id = 1,
            OrderId = "ORD-10452",
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        var order2 = new Order
        {
            Id = 2,
            OrderId = "ORD-10453",
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.AddRange(order1, order2);

        // Seed OrderItems (explicit IDs to avoid duplicates)
        context.OrderItems.AddRange(
            new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, PickedQuantity = 0 },
            new OrderItem { Id = 2, OrderId = 2, ProductId = 2, Quantity = 3, PickedQuantity = 0 }
        );

        context.SaveChanges();
    }

    public new System.Net.Http.HttpClient CreateClient(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions options)
    {
        // Re-seed DB for each client to ensure test isolation between tests
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        SeedTestData(db);
        return base.CreateClient(options);
    }

    public new System.Net.Http.HttpClient CreateClient()
    {
        return CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions());
    }
}
