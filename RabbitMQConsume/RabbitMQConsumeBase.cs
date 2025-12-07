using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace RabbitMQConsume
{
    /*
      This class cannot be instantiated directly. You must inherit from it to create a concrete consumer.
      IDisposable: Implements IDisposable, so it can clean up resources like RabbitMQ connections.
      where T : class: Constraint — T must be a reference type.
      
     */
    public abstract class RabbitMQConsumeBase<T> : IDisposable where T : class
    {
        private readonly string _queueName;//Name of the queue this consumer reads from.
        private readonly IConnection _connection;//RabbitMQ connection object.
        private readonly IModel _channel;//Channel object used to communicate with RabbitMQ.
        private bool _disposed = false;//Tracks whether resources have been cleaned up (for IDisposable).

        //protected: Only subclasses can call this constructor.
        protected RabbitMQConsumeBase(string hostName, int port, string userName, string password, string queueName,string exchangeName = null, string exchangeType = "fanout")
        {
            _queueName = queueName;

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                DispatchConsumersAsync = true,//allows async event handlers.
                AutomaticRecoveryEnabled = true,//auto-reconnects if connection fails.
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)//wait time between reconnect attempts.
            };

            //Creates a connection and a channel. Every consumer needs a channel to talk to RabbitMQ.
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();


            if (!string.IsNullOrEmpty(exchangeName))
            {
                _channel.ExchangeDeclare(exchangeName, exchangeType, durable: true, autoDelete: false);//autoDelete: false → queue/exchange is not deleted automatically.
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);//exclusive: false → allows multiple consumers.
                _channel.QueueBind(queueName, exchangeName, ""); // fanout ignores routing key
            }
            else
            {
                // just declare the queue
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            }
    
        }



        //Begins consuming messages from RabbitMQ.
        //Optional CancellationToken allows graceful shutdown.
        public void StartConsuming(CancellationToken cancellationToken = default)
        {
            /*
             * AsyncEventingBasicConsumer is a special RabbitMQ consumer class designed for asynchronous handling of messages.
             * When you receive messages, you might do async operations (like database writes, HTTP requests, or other I/O) inside HandleMessageAsync.
             * Using a normal synchronous consumer would block the thread while waiting for I/O, limiting scalability.
             * Async + AsyncEventingBasicConsumer = scalable, non-blocking message processing.
             */
            var consumer = new AsyncEventingBasicConsumer(_channel);

            /*
             * event subscription with an async lambda
             * Received is an event exposed by AsyncEventingBasicConsumer.
             * Events in C# follow the observer pattern — you can subscribe methods to be called whenever the event is triggered.
             * Received is triggered every time a message arrives in the queue.
             * += operator attaches a handler to the event.
             * (model, ea) → parameters passed by the event:
             * model → usually the consumer itself or sender information.
             * ea → BasicDeliverEventArgs, contains the message body, routing info, delivery tag, etc.
             * 
             */


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
                    _channel.BasicAck(ea.DeliveryTag, false);//confirms message was processed successfully.
                }
                catch (Exception ex)
                {
                    // Failed messages go back to queue
                    Console.WriteLine($"Error in consumer: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);//failed message, requeue it.
                }
            };

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine("OrderConsumer connected to RabbitMQ"); // <-- your log line;

            /*
             * Task.Run(() => { ... })
             * Starts a new background thread (task) to run the code inside the lambda.
             * This is asynchronous from the main program; it won’t block the main thread.
             * cancellationToken is a way for the caller to signal “stop”.
             * WaitHandle.WaitOne() blocks this background thread until the token is cancelled.
             * It’s a way to let the consumer run in the background without blocking the main program, and also ensures proper cleanup.
             * With Task.Run:
             * The WaitOne() runs on a separate background thread.
             * Your main thread (the one running the HTTP server and controllers) remains free to handle requests.
             * When a cancellation happens, the background thread calls Dispose(), cleaning up resources without blocking the main app.
             * In short: it’s a non-blocking way to wait for a stop signal while letting your service continue working normally.
             * The cancellationToken is an object of type CancellationToken. It’s part of .NET’s way to signal “stop” or “cancel” long-running operations safely.
             * It allows graceful shutdown: your background consumer can clean up connections, channels, and resources before the app exits.
             * The main thread doesn’t get blocked because WaitOne() is run inside Task.Run() on a separate thread.

             */
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
