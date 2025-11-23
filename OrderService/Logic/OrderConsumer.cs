using OrderService.Logic.Interfaces;
using SharedDTOs;
using RabbitMQConsume;
using System.Text.Json;

namespace OrderService.Logic
{
    public class OrderConsumer : RabbitMQConsumeBase<OrderDTO>, IOrderConsumer
    {
        private readonly ILogger<OrderConsumer> ?_logger;
        public static readonly List<OrderDTO> _orders = new List<OrderDTO>();
        private static readonly object _lock = new object();
        public OrderConsumer(string hostName, string queueName, ILogger<OrderConsumer> logger) : base(hostName, queueName)
        {
            _logger = logger;
        }

        public override async Task HandleMessageAsync(OrderDTO orderDTO)
        {
            try
            {
                // Example: log the order
                _logger.LogInformation($"Received Order: {orderDTO.OrderId}, Total: {orderDTO.Totals}");
                await ProcessOrderAsync(orderDTO);
                // TODO: implement your order processing logic here
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order");
                throw; // will trigger BasicNack in base class
            }
        }

        public async Task ProcessOrderAsync(OrderDTO orderDTO)
        {

            // 1. Calculate shipping cost
            orderDTO.Shipping.ShippingCost = (int)orderDTO.Totals.TotalAmount * (int)0.02M;

            // 2. Save to in-memory list
            lock (_lock)
            {
                _orders.Add(orderDTO);
            }

            // 3. Save to JSON file
            string fileName = $"order_{orderDTO.OrderId}.json";
            string json = JsonSerializer.Serialize(orderDTO, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(fileName, json);

            _logger.LogInformation($"Order {orderDTO.OrderId} processed: Shipping={orderDTO.Shipping.ShippingCost}, saved to {fileName}");

        }









    }
}
