using OrderService.Logic.Interfaces;
using RabbitMQ.Client;
using SharedDTOs;
namespace OrderService.Logic
{
    public class OrderConsumer : IOrderConsumer, IDisposable
    {
        private IModel? _channel;
        private IConnection? _connection;
        private readonly ILogger<OrderConsumer> _logger;
        private bool _disposed = false;


        //public OrderConsumer()

        


        public void Dispose()
        {
            if(!_disposed)
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _disposed = true;
                _logger.LogInformation("RabbitMQ connection closed");
 
            }
        }
    }
}
