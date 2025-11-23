using OrderService.Logic.Interfaces;
using SharedDTOs;
using RabbitMQConsume;
using System.Text.Json;
using OrderService.Services;

namespace OrderService.Logic
{
    public class OrderConsumer : RabbitMQConsumeBase<OrderDTO>, IOrderConsumer
    {
        private readonly ILogger<OrderConsumer> ?_logger;
      //  public static readonly List<OrderDTO> _orders = new List<OrderDTO>();
        private static readonly object _lock = new object();
        private readonly ProcessedOrdersStore ?_store;
        public OrderConsumer(string hostName, string queueName, ILogger<OrderConsumer> logger) : base(hostName, queueName)
        {
            _logger = logger;
        }

        public override async Task HandleMessageAsync(OrderDTO orderDTO)
        {
            try
            {
                // Example: log the order
                _logger?.LogInformation($"Received Order: {orderDTO.OrderId}, Total: {orderDTO.Totals}");
                await ProcessOrderAsync(orderDTO);
                _store?.AddOrder(orderDTO);
                // TODO: implement your order processing logic here
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing order");
                throw; // will trigger BasicNack in base class
            }
        }

        public async Task ProcessOrderAsync(OrderDTO orderDTO)
        {

            // 1. Calculate shipping cost
            orderDTO.Shipping.ShippingCost = orderDTO.Totals.TotalAmount * 0.10;

            // 3. Save to JSON file
            string fileName = $"order_{orderDTO.OrderId}.json";
            string json = JsonSerializer.Serialize(orderDTO, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(fileName, json);
            _store.AddOrder(orderDTO);
            _logger.LogInformation($"Order {orderDTO.OrderId} processed: Shipping={orderDTO.Shipping.ShippingCost}, saved to {fileName}");

        }









    }
}
