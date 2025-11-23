using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace RabbitMQConsume
{
    public abstract class RabbitMQConsumeBase<T> : IDisposable where T : class
    {
        private readonly string _queueName;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private bool _disposed = false;

        protected RabbitMQConsumeBase(string hostName, string queueName)
        {
            _queueName = queueName;

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Ensure queue exists
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        public void StartConsuming(CancellationToken cancellationToken = default)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    string json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var messageObj = JsonSerializer.Deserialize<T>(json);

                    if (messageObj == null)
                    {
                        throw new Exception("Failed to deserialize message.");
                    }
                       
                    await HandleMessageAsync(messageObj);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch
                {
                    // Failed messages go back to queue
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            // Optional: let caller decide when to stop
            Task.Run(() =>
            {
                cancellationToken.WaitHandle.WaitOne();
                Dispose();
            });
        }


       
        // Override this in each consumer to define custom behavior.
        
        public abstract Task HandleMessageAsync(T message);

        public void Dispose()
        {
            if (_disposed) return;

            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _disposed = true;
        }

    }
}
