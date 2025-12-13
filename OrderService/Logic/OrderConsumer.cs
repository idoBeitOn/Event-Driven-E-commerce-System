using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Data.Entities;
using OrderService.Services;
using RabbitMQConsume;
using SharedDTOs;
namespace OrderService.Logic
{
    public class OrderConsumer : RabbitMQConsumeBase<OrderDTO>
    {
        private readonly ILogger<OrderConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Used to create scoped services (DbContext) inside this singleton consumer

        public OrderConsumer(
        string hostName,
        int port,
        string userName,
        string password,
        string queueName,
        string exchangeName,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderConsumer> logger
    )
    : base(hostName, port, userName, password, queueName, exchangeName, "fanout")
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        public override async Task HandleMessageAsync(OrderDTO orderDTO)
        {
            using (Serilog.Context.LogContext.PushProperty("OrderID", orderDTO.OrderId)) 
            {
                try
                {
                    _logger.LogInformation("Order message received. Total={TotalAmount}", orderDTO.Totals.TotalAmount);

                    // Process the order (calculate shipping, persist to DB)
                    await ProcessOrderAsync(orderDTO);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order");
                    throw; // triggers BasicNack in the base class
                }
            }
            
        }

        private async Task ProcessOrderAsync(OrderDTO orderDTO)
        {
            // IMPORTANT: DbContext is scoped (not thread-safe), while this consumer is singleton.
            // We create a scope per message to safely resolve a fresh DbContext instance.
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            // Step 1: Prevent duplicates (e.g., if RabbitMQ redelivers the same message)
            var existing = await db.Orders.FindAsync(orderDTO.OrderId);
            if (existing != null)
            {
                _logger.LogWarning("Order {OrderId} already exists. Skipping duplicate.", orderDTO.OrderId);
                return; // Do not process duplicates
            }

            // Step 2: Calculate shipping (simple rule: 10% of total)
            orderDTO.Shipping.ShippingCost = orderDTO.Totals.TotalAmount * 0.10;

            // Step 3: Map DTO → Entity for persistence
            var orderEntity = MapToEntity(orderDTO);

            // Step 4: Save to database
            db.Orders.Add(orderEntity);
            await db.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} processed and saved. Shipping={ShippingCost}", orderDTO.OrderId, orderDTO.Shipping.ShippingCost);
        }

        /// <summary>
        /// Maps the incoming OrderDTO (from RabbitMQ) into our EF Core entities.
        /// Keeps persistence concerns separated from the DTO transport model.
        /// </summary>
        private static Order MapToEntity(OrderDTO dto)
        {
            var order = new Order
            {
                OrderId = dto.OrderId,
                Status = dto.Status,
                CreatedAt = dto.CreatedAt,

                // Customer (denormalized into order row)
                CustomerId = dto.Customer.CustomerId,
                CustomerName = dto.Customer.Name,
                CustomerEmail = dto.Customer.Email,
                CustomerAddress = dto.Customer.Address,
                CustomerCity = dto.Customer.City,
                CustomerCountry = dto.Customer.Country,
                CustomerPostalCode = dto.Customer.PostalCode,

                // Totals
                SubTotal = (decimal)dto.Totals.SubTotal,
                Tax = (decimal)dto.Totals.Tax,
                Discount = (decimal)dto.Totals.Discount,
                TotalAmount = (decimal)dto.Totals.TotalAmount,
                Currency = dto.Totals.Currency,

                // Shipping
                ShippingMethod = dto.Shipping.Method,
                ShippingCost = (decimal)dto.Shipping.ShippingCost,
                ShippingAddress = dto.Shipping.Address,
                ShippingCity = dto.Shipping.City,
                ShippingCountry = dto.Shipping.Country,
                ShippingPostalCode = dto.Shipping.PostalCode,
                EstimatedDeliveryDate = dto.Shipping.EstimatedDeliveryDate,

                // Payment
                PaymentMethod = dto.Payment.Method,
                TransactionId = dto.Payment.TransactionId,
                Paid = dto.Payment.Paid,

                // Metadata
                Source = dto.Metadata.Source,
                Version = dto.Metadata.Version,

                ItemsNum = dto.itemsNum
            };

            // Map line items
            order.Items = dto.Items.Select(item => new OrderItem
            {
                OrderId = dto.OrderId,
                LineId = item.LineId,
                Name = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Currency = item.Currency,
                Category = item.Category
            }).ToList();

            return order;
        }
    }
}
