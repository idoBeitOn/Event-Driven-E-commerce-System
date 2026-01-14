using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Data;
using OrderService.Logic;
using SharedDTOs;
using Xunit;

namespace OrderService.Tests;

/*
 * Unit Tests for OrderConsumer
 * 
 * These tests verify that OrderConsumer correctly:
 * 1. Saves new orders to the database
 * 2. Skips duplicate orders (idempotency)
 * 3. Calculates shipping costs correctly
 * 4. Maps DTOs to entities correctly
 * 
 * We use InMemory database for fast, isolated tests.
 */

public class OrderConsumerTests
{
    /*
     * Helper method to create an in-memory DbContext for testing.
     * Each test gets a fresh, isolated database.
     */
    private OrderDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name per test
            .Options;

        return new OrderDbContext(options);
    }

    /*
     * Helper method to create a test OrderDTO with sample data.
     */
    private OrderDTO CreateTestOrder(string orderId = "ORD-TEST-001")
    {
        return new OrderDTO
        {
            OrderId = orderId,
            Status = "new",
            CreatedAt = DateTime.UtcNow,
            Customer = new CustomerDTO
            {
                CustomerId = "CUST-001",
                Name = "John Doe",
                Email = "john@example.com",
                Address = "123 Main St",
                City = "New York",
                Country = "USA",
                PostalCode = "10001"
            },
            Items = new List<OrderItemDTO>
            {
                new OrderItemDTO
                {
                    LineId = "LINE-001",
                    Name = "Laptop",
                    Quantity = 1,
                    UnitPrice = 1000.00m,
                    Currency = "USD",
                    Category = "Electronics"
                }
            },
            itemsNum = 1,
            Totals = new TotalsDTO
            {
                SubTotal = 1000.0,
                Tax = 80.0,
                Discount = 0.0,
                TotalAmount = 1080.0,
                Currency = "USD"
            },
            Payment = new PaymentDTO
            {
                Method = "credit_card",
                TransactionId = "TXN-001",
                Paid = false
            },
            Shipping = new ShippingDTO
            {
                Method = "standard",
                ShippingCost = 0, // Will be calculated
                Address = "123 Main St",
                City = "New York",
                Country = "USA",
                PostalCode = "10001",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5)
            },
            Metadata = new MetadataDTO
            {
                Source = "cart-service",
                Version = 1
            }
        };
    }

    [Fact]
    public async Task ProcessOrderAsync_SavesNewOrder_ToDatabase()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var serviceProvider = new Mock<IServiceProvider>();

        // Setup DI chain: scopeFactory → scope → serviceProvider → dbContext
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);
        scope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
        serviceProvider.Setup(x => x.GetService(typeof(OrderDbContext))).Returns(db);

        var logger = new Mock<ILogger<OrderConsumer>>();
        var consumer = new OrderConsumer("localhost", 5672, "guest", "guest", "test-queue", "test-exchange", scopeFactory.Object, logger.Object);

        var orderDto = CreateTestOrder();

        // Act
        await consumer.HandleMessageAsync(orderDto);

        // Assert
        var savedOrder = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == "ORD-TEST-001");
        Assert.NotNull(savedOrder);
        Assert.Equal("ORD-TEST-001", savedOrder.OrderId);
        Assert.Equal("John Doe", savedOrder.CustomerName);
        Assert.Equal(1080.0m, savedOrder.TotalAmount);
        Assert.Equal(108.0m, savedOrder.ShippingCost); // 10% of 1080
        Assert.Single(savedOrder.Items);
        Assert.Equal("Laptop", savedOrder.Items.First().Name);
    }

    [Fact]
    public async Task ProcessOrderAsync_SkipsDuplicateOrder_WhenOrderExists()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        
        // Pre-populate database with existing order
        var existingOrder = new OrderService.Data.Entities.Order
        {
            OrderId = "ORD-EXISTING",
            Status = "processed",
            CreatedAt = DateTime.UtcNow,
            CustomerName = "Jane Doe",
            TotalAmount = 500.0m,
            ShippingCost = 50.0m
        };
        db.Orders.Add(existingOrder);
        await db.SaveChangesAsync();

        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var serviceProvider = new Mock<IServiceProvider>();

        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);
        scope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
        serviceProvider.Setup(x => x.GetService(typeof(OrderDbContext))).Returns(db);

        var logger = new Mock<ILogger<OrderConsumer>>();
        var consumer = new OrderConsumer("localhost", 5672, "guest", "guest", "test-queue", "test-exchange", scopeFactory.Object, logger.Object);

        var orderDto = CreateTestOrder("ORD-EXISTING");

        // Act
        await consumer.HandleMessageAsync(orderDto);

        // Assert - Verify only one order exists (no duplicate created)
        var orders = await db.Orders.Where(o => o.OrderId == "ORD-EXISTING").ToListAsync();
        Assert.Single(orders);
        Assert.Equal("processed", orders.First().Status); // Original status preserved
    }

    [Fact]
    public async Task ProcessOrderAsync_CalculatesShippingCost_As10PercentOfTotal()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var serviceProvider = new Mock<IServiceProvider>();

        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);
        scope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
        serviceProvider.Setup(x => x.GetService(typeof(OrderDbContext))).Returns(db);

        var logger = new Mock<ILogger<OrderConsumer>>();
        var consumer = new OrderConsumer("localhost", 5672, "guest", "guest", "test-queue", "test-exchange", scopeFactory.Object, logger.Object);

        var orderDto = CreateTestOrder();
        orderDto.Totals.TotalAmount = 2000.0; // Set total to $2000

        // Act
        await consumer.HandleMessageAsync(orderDto);

        // Assert
        var savedOrder = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == "ORD-TEST-001");
        Assert.NotNull(savedOrder);
        Assert.Equal(200.0m, savedOrder.ShippingCost); // 10% of $2000 = $200
    }
}
