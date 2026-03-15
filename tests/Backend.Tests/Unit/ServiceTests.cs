namespace Backend.Tests.Unit;

using Backend.Data;
using Backend.Data.Models;
using Backend.Business.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class PickServiceTests
{
    [Fact]
    public async Task ConfirmPick_WithValidData_UpdatesInventory()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new WarehouseDbContext(options))
        {
            var product = new Product { Id = 1, Sku = "TEST-SKU", Barcode = "123456789" };
            var location = new Location { Id = 1, Rack = 5, Slot = 2 };
            var order = new Order { Id = 1, OrderId = "ORD-001", Status = "pending" };
            var orderItem = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, PickedQuantity = 0 };
            var inventory = new Inventory { ProductId = 1, LocationId = 1, Quantity = 5 };

            context.Products.Add(product);
            context.Locations.Add(location);
            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);
            context.Inventory.Add(inventory);
            context.SaveChanges();
        }

        using (var context = new WarehouseDbContext(options))
        {
            var service = new PickService(context);
            var result = await service.ConfirmPick("ORD-001", "123456789", 5, 2);

            Assert.True(result);
            var updatedInventory = context.Inventory.First();
            Assert.Equal(4, updatedInventory.Quantity);
            var updatedItem = context.OrderItems.First();
            Assert.Equal(1, updatedItem.PickedQuantity);
        }
    }

    [Fact]
    public async Task ConfirmPick_WithInsufficientInventory_ThrowsException()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new WarehouseDbContext(options))
        {
            var product = new Product { Id = 1, Sku = "TEST-SKU", Barcode = "123456789" };
            var location = new Location { Id = 1, Rack = 5, Slot = 2 };
            var order = new Order { Id = 1, OrderId = "ORD-001", Status = "pending" };
            var orderItem = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, PickedQuantity = 0 };
            var inventory = new Inventory { ProductId = 1, LocationId = 1, Quantity = 0 };

            context.Products.Add(product);
            context.Locations.Add(location);
            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);
            context.Inventory.Add(inventory);
            context.SaveChanges();
        }

        using (var context = new WarehouseDbContext(options))
        {
            var service = new PickService(context);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConfirmPick("ORD-001", "123456789", 5, 2));
        }
    }

    [Fact]
    public async Task ConfirmPick_LogsScanEvent()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new WarehouseDbContext(options))
        {
            var product = new Product { Id = 1, Sku = "TEST-SKU", Barcode = "123456789" };
            var location = new Location { Id = 1, Rack = 5, Slot = 2 };
            var order = new Order { Id = 1, OrderId = "ORD-001", Status = "pending" };
            var orderItem = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, PickedQuantity = 0 };
            var inventory = new Inventory { ProductId = 1, LocationId = 1, Quantity = 5 };

            context.Products.Add(product);
            context.Locations.Add(location);
            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);
            context.Inventory.Add(inventory);
            context.SaveChanges();
        }

        using (var context = new WarehouseDbContext(options))
        {
            var service = new PickService(context);
            await service.ConfirmPick("ORD-001", "123456789", 5, 2);

            var scanEvent = context.ScanEvents.FirstOrDefault();
            Assert.NotNull(scanEvent);
            Assert.Equal(1, scanEvent.ProductId);
            Assert.Equal("5", scanEvent.Rack);
            Assert.Equal("2", scanEvent.Slot);
            Assert.Equal("Pick", scanEvent.Action);
        }
    }
}

public class InventoryServiceTests
{
    [Fact]
    public async Task GetInventory_WithValidProductAndLocation_ReturnsQuantity()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new WarehouseDbContext(options))
        {
            var product = new Product { Id = 1, Sku = "TEST-SKU" };
            var location = new Location { Id = 1, Rack = 5, Slot = 2 };
            var inventory = new Inventory { ProductId = 1, LocationId = 1, Quantity = 42 };

            context.Products.Add(product);
            context.Locations.Add(location);
            context.Inventory.Add(inventory);
            context.SaveChanges();
        }

        using (var context = new WarehouseDbContext(options))
        {
            var service = new InventoryService(context);
            var quantity = await service.GetInventory(1, 1);

            Assert.Equal(42, quantity);
        }
    }

    [Fact]
    public async Task ReduceInventory_WithValidData_DecrementsQuantity()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        using (var context = new WarehouseDbContext(options))
        {
            var product = new Product { Id = 1, Sku = "TEST-SKU" };
            var location = new Location { Id = 1, Rack = 5, Slot = 2 };
            var inventory = new Inventory { ProductId = 1, LocationId = 1, Quantity = 10 };

            context.Products.Add(product);
            context.Locations.Add(location);
            context.Inventory.Add(inventory);
            context.SaveChanges();
        }

        using (var context = new WarehouseDbContext(options))
        {
            var service = new InventoryService(context);
            var result = await service.ReduceInventory(1, 1, 3);

            Assert.True(result);
            var updatedInventory = context.Inventory.First();
            Assert.Equal(7, updatedInventory.Quantity);
        }
    }
}

public class OrderServiceTests
{
    [Fact]
    public async Task GetOrderById_WithValidId_ReturnsOrder()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new WarehouseDbContext(options))
        {
            var order = new Order { Id = 1, OrderId = "ORD-001", Status = "pending" };
            context.Orders.Add(order);
            context.SaveChanges();
        }

        using (var context = new WarehouseDbContext(options))
        {
            var service = new OrderService(context);
            var result = await service.GetOrderById("ORD-001");

            Assert.NotNull(result);
            Assert.Equal("ORD-001", result.OrderId);
            Assert.Equal("pending", result.Status);
        }
    }

    [Fact]
    public async Task GetOrderById_WithNonExistentId_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new WarehouseDbContext(options))
        {
            var service = new OrderService(context);
            var result = await service.GetOrderById("NONEXISTENT");

            Assert.Null(result);
        }
    }
}
