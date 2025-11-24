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

        protected RabbitMQConsumeBase(string hostName, int port, string userName, string password, string queueName,string exchangeName = null, string exchangeType = "fanout")
        {
            _queueName = queueName;

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();


            if (!string.IsNullOrEmpty(exchangeName))
            {
                _channel.ExchangeDeclare(exchangeName, exchangeType, durable: true, autoDelete: false);
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(queueName, exchangeName, ""); // fanout ignores routing key
            }
            else
            {
                // just declare the queue
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            }
    
        }

        public void StartConsuming(CancellationToken cancellationToken = default)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    
                    string json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine("RAW JSON:");
                    Console.WriteLine(json);
                    var messageObj = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (messageObj == null)
                    {
                        throw new Exception("Failed to deserialize message.");
                    }
                       
                    await HandleMessageAsync(messageObj);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // Failed messages go back to queue
                    Console.WriteLine($"Error in consumer: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine("OrderConsumer connected to RabbitMQ"); // <-- your log line;

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
