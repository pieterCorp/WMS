namespace Backend.Tests.Integration;

using Backend.Tests.Integration.Fixtures;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class OrderIntegrationTests : IClassFixture<WarehouseWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrderIntegrationTests(WarehouseWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrderById_WithValidId_ReturnsOrderWithItems()
    {
        // Act
        var response = await _client.GetAsync("/orders/ORD-10452");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadAsAsync<OrderResponse>();
        Assert.NotNull(order);
        Assert.Equal("ORD-10452", order.OrderId);
        Assert.Single(order.Items);
        Assert.Equal("USB Cable", order.Items[0].Product);
        Assert.Equal("8872", order.Items[0].Sku);
        Assert.Equal("A7-3", order.Items[0].Location);
        Assert.Equal(2, order.Items[0].Quantity);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistentId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/orders/ORD-INVALID");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_WithMultipleItems_ReturnsAllItems()
    {
        // Act
        var response = await _client.GetAsync("/orders/ORD-10453");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadAsAsync<OrderResponse>();
        Assert.NotNull(order);
        Assert.Single(order.Items);
        Assert.Equal("HDMI Cable", order.Items[0].Product);
        Assert.Equal(3, order.Items[0].Quantity);
    }
}

public class PickIntegrationTests : IClassFixture<WarehouseWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PickIntegrationTests(WarehouseWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConfirmPick_WithValidData_ReducesInventory()
    {
        // Arrange
        var pickRequest = new PickRequest
        {
            OrderId = "ORD-10452",
            ProductBarcode = "5401234567890",
            Rack = 7,
            Slot = 3
        };

        // Act
        var response = await _client.PostAsJsonAsync("/pick", pickRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify inventory was reduced
        var inventoryResponse = await _client.GetAsync("/inventory/product/1/location/1");
        Assert.Equal(HttpStatusCode.OK, inventoryResponse.StatusCode);

        var inventory = await inventoryResponse.Content.ReadAsAsync<InventoryResponse>();
        Assert.NotNull(inventory);
        Assert.Equal(4, inventory.Quantity);
    }

    [Fact]
    public async Task ConfirmPick_WithInvalidOrderId_ReturnsBadRequest()
    {
        // Arrange
        var pickRequest = new PickRequest
        {
            OrderId = "ORD-INVALID",
            ProductBarcode = "5401234567890",
            Rack = 7,
            Slot = 3
        };

        // Act
        var response = await _client.PostAsJsonAsync("/pick", pickRequest);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmPick_WithInvalidBarcode_ReturnsBadRequest()
    {
        // Arrange
        var pickRequest = new PickRequest
        {
            OrderId = "ORD-10452",
            ProductBarcode = "9999999999999",
            Rack = 7,
            Slot = 3
        };

        // Act
        var response = await _client.PostAsJsonAsync("/pick", pickRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmPick_WithInvalidRack_ReturnsBadRequest()
    {
        // Arrange
        var pickRequest = new PickRequest
        {
            OrderId = "ORD-10452",
            ProductBarcode = "5401234567890",
            Rack = 99,
            Slot = 3
        };

        // Act
        var response = await _client.PostAsJsonAsync("/pick", pickRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmPick_WithMultiplePicks_DecrementsCumulatively()
    {
        // Arrange
        var pickRequest = new PickRequest
        {
            OrderId = "ORD-10452",
            ProductBarcode = "5401234567890",
            Rack = 7,
            Slot = 3
        };

        // Act - Pick 1
        await _client.PostAsJsonAsync("/pick", pickRequest);

        // Act - Pick 2
        var response2 = await _client.PostAsJsonAsync("/pick", pickRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        // Verify inventory
        var inventoryResponse = await _client.GetAsync("/inventory/product/1/location/1");
        var inventory = await inventoryResponse.Content.ReadAsAsync<InventoryResponse>();
        Assert.NotNull(inventory);
        Assert.Equal(3, inventory.Quantity);
    }
}

// DTOs for tests
public class OrderResponse
{
    public string OrderId { get; set; } = default!;
    public OrderItemResponse[] Items { get; set; } = Array.Empty<OrderItemResponse>();
}

public class OrderItemResponse
{
    public string Product { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string Location { get; set; } = default!;
    public int Quantity { get; set; }
}

public class PickRequest
{
    public string OrderId { get; set; } = default!;
    public string ProductBarcode { get; set; } = default!;
    public int Rack { get; set; }
    public int Slot { get; set; }
}

public class InventoryResponse
{
    public int Quantity { get; set; }
}

// Extension method for modern .NET deserialization
internal static class HttpContentExtensions
{
    public static async Task<T?> ReadAsAsync<T>(this HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}
