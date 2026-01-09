using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Controllers;
using OrderService.Data;
using OrderService.Data.Entities;
using Xunit;

namespace OrderService.Tests;

/*
 * Integration Tests for OrdersController
 * 
 * These tests verify that the API endpoint:
 * 1. Returns 404 for non-existent orders
 * 2. Returns correct order summary for existing orders
 * 3. Includes all required fields in response
 * 
 * Uses InMemory database to simulate real database behavior.
 */

public class OrdersControllerTests
{
    /*
     * Helper method to create an in-memory DbContext for testing.
     */
    private OrderDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    /*
     * Helper method to seed test data into the database.
     */
    private async Task SeedTestDataAsync(OrderDbContext db)
    {
        var order = new Order
        {
            OrderId = "ORD-TEST-001",
            Status = "processed",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CustomerId = "CUST-001",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            CustomerAddress = "123 Main St",
            CustomerCity = "New York",
            CustomerCountry = "USA",
            CustomerPostalCode = "10001",
            SubTotal = 1000.0m,
            Tax = 80.0m,
            Discount = 0.0m,
            TotalAmount = 1080.0m,
            Currency = "USD",
            ShippingMethod = "standard",
            ShippingCost = 108.0m,
            ShippingAddress = "123 Main St",
            ShippingCity = "New York",
            ShippingCountry = "USA",
            ShippingPostalCode = "10001",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
            PaymentMethod = "credit_card",
            TransactionId = "TXN-001",
            Paid = false,
            Source = "cart-service",
            Version = 1,
            ItemsNum = 2,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderId = "ORD-TEST-001",
                    LineId = "LINE-001",
                    Name = "Laptop",
                    Quantity = 1,
                    UnitPrice = 1000.0m,
                    Currency = "USD",
                    Category = "Electronics"
                },
                new OrderItem
                {
                    OrderId = "ORD-TEST-001",
                    LineId = "LINE-002",
                    Name = "Mouse",
                    Quantity = 1,
                    UnitPrice = 50.0m,
                    Currency = "USD",
                    Category = "Accessories"
                }
            }
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetOrderSummary_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var logger = new Mock<ILogger<OrdersController>>();
        var controller = new OrdersController(db, logger.Object);

        // Act
        var result = await controller.GetOrderSummary("ORD-NONEXISTENT");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.Equal(404, notFoundResult?.StatusCode);
    }

    [Fact]
    public async Task GetOrderSummary_ReturnsOrderSummary_WhenOrderExists()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        await SeedTestDataAsync(db);

        var logger = new Mock<ILogger<OrdersController>>();
        var controller = new OrdersController(db, logger.Object);

        // Act
        var result = await controller.GetOrderSummary("ORD-TEST-001");

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var summary = okResult?.Value as OrderSummaryResponse;

        Assert.NotNull(summary);
        Assert.Equal("ORD-TEST-001", summary.OrderId);
        Assert.Equal(1080.0m, summary.TotalAmount);
        Assert.Equal(108.0m, summary.ShippingCost);
        Assert.Equal("John Doe", summary.CustomerName);
        Assert.Equal(2, summary.NumberOfItems); // Two items in test data
    }
}
