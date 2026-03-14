using System;
using System.Linq;
using Backend.Models;

namespace Backend.Data
{
    public static class DbSeeder
    {
        public static void Seed(WarehouseDbContext context)
        {
            // If data exists, do nothing
            if (context.Products.Any())
                return;

            // Products
            var p1 = new Product { Sku = "8872", Name = "USB Cable", Barcode = "5401234567890" };
            var p2 = new Product { Sku = "9001", Name = "HDMI Cable", Barcode = "5401234567891" };
            context.Products.AddRange(p1, p2);
            context.SaveChanges();

            // Locations
            var loc1 = new Location { Aisle = "A", Rack = 7, Slot = 3, X = 10m, Y = 5m, Z = 1m };
            var loc2 = new Location { Aisle = "B", Rack = 2, Slot = 1, X = 20m, Y = 3m, Z = 2m };
            context.Locations.AddRange(loc1, loc2);
            context.SaveChanges();

            // Inventory
            context.Inventory.AddRange(
                new Inventory { ProductId = p1.Id, LocationId = loc1.Id, Quantity = 5 },
                new Inventory { ProductId = p2.Id, LocationId = loc2.Id, Quantity = 10 }
            );
            context.SaveChanges();

            // Orders
            var order1 = new Order { OrderId = "ORD-10452", Status = "pending", CreatedAt = DateTime.UtcNow };
            var order2 = new Order { OrderId = "ORD-10453", Status = "pending", CreatedAt = DateTime.UtcNow };
            context.Orders.AddRange(order1, order2);
            context.SaveChanges();

            // OrderItems
            context.OrderItems.AddRange(
                new OrderItem { OrderId = order1.Id, ProductId = p1.Id, Quantity = 2, PickedQuantity = 0 },
                new OrderItem { OrderId = order2.Id, ProductId = p2.Id, Quantity = 3, PickedQuantity = 0 }
            );

            context.SaveChanges();
        }
    }
}
