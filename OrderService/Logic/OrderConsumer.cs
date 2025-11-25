using OrderService.Services;
using SharedDTOs;
using RabbitMQConsume;
using System.Text.Json;
using System.IO;
namespace OrderService.Logic
{
    public class OrderConsumer : RabbitMQConsumeBase<OrderDTO>
    {
        private readonly ILogger<OrderConsumer> _logger;
        private readonly ProcessedOrdersStore _store;

        public OrderConsumer(
        string hostName,
        int port,
        string userName,
        string password,
        string queueName,
        string exchangeName,
        ProcessedOrdersStore store,
        ILogger<OrderConsumer> logger
    )
    : base(hostName, port, userName, password, queueName, exchangeName, "fanout")
        {
            _store = store;
            _logger = logger;
        }










        /*
                public OrderConsumer(IConfiguration configuration, ILogger<OrderConsumer> logger, ProcessedOrdersStore store)
                 : base(configuration["RabbitMQ:HostName"] ?? "rabbitmq",
                       int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                       configuration["RabbitMQ:UserName"] ?? "guest",
                       configuration["RabbitMQ:Password"] ?? "guest",
                       configuration["RabbitMQ:QueueName"] ?? "order-queue")
                {
                    _logger = logger;
                    _store = store;
                }

                */

        public override async Task HandleMessageAsync(OrderDTO orderDTO)
        {
            try
            {
                _logger.LogInformation($"Received Order: {orderDTO.OrderId}, Total: {orderDTO.Totals.TotalAmount}");

                // Process order (calculations, file saving, store in memory)
                await ProcessOrderAsync(orderDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order");
                throw; // triggers BasicNack in the base class
            }
        }

        private async Task ProcessOrderAsync(OrderDTO orderDTO)
        {
            // 1. Calculate shipping cost (10% of total)
            orderDTO.Shipping.ShippingCost = orderDTO.Totals.TotalAmount * 0.10;

            // 2. Save order to a JSON file
            string fileName = $"order_{orderDTO.OrderId}.json";
            string json = JsonSerializer.Serialize(orderDTO, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(fileName, json);

            // 3. Add to in-memory store
            _store.AddOrder(orderDTO);

            _logger.LogInformation($"Order {orderDTO.OrderId} processed: Shipping={orderDTO.Shipping.ShippingCost}, saved to {fileName}");
        }
    }
}
