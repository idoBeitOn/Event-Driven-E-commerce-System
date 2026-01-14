using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using SharedDTOs;
namespace Cart_Service.Services;

/// <summary>
/// OrderPublisher - Handles publishing order events to RabbitMQ
/// Follows Single Responsibility Principle: Only handles RabbitMQ publishing
/// </summary>
public class OrderPublisher : IOrderPublisher, IDisposable
{
    private IConnection? _connection;//TCP connection to RabbitMQ server
    private IModel? _channel;//an AMQP channel 
    private readonly ILogger<OrderPublisher> _logger;
    private readonly ConnectionFactory _factory;//creates connections
    private readonly string _exchangeName;
    private readonly string _exchangeType;
    private readonly string _routingKey;
    private readonly object _lock = new object();//ensures thread safety if multiple threads publish simultaneously
    private bool _disposed = false;//tracks if Dispose() was already called
    
    /*
     * Resilience Policy - Retry with exponential backoff
     * 
     * This policy will retry failed RabbitMQ operations up to 3 times.
     * Wait time between retries increases exponentially: 1s, 2s, 4s
     * 
     * Why this is important:
     * - Network hiccups can cause temporary connection failures
     * - RabbitMQ might be briefly unavailable during restarts
     * - Exponential backoff prevents overwhelming a recovering service
     * 
     * This demonstrates production-ready error handling patterns.
     */
    private readonly AsyncRetryPolicy _retryPolicy;

    public OrderPublisher(IConfiguration configuration, ILogger<OrderPublisher> logger)
    {
        _logger = logger;
        
        // Read RabbitMQ configuration from appsettings.json
        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var userName = configuration["RabbitMQ:UserName"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";
        _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "order-exchange";
        _exchangeType = configuration["RabbitMQ:ExchangeType"] ?? "fanout";
        _routingKey = configuration["RabbitMQ:RoutingKey"] ?? "order.new";

        // Create connection factory (lazy connection - won't connect until needed)
        _factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };

        _logger.LogInformation(
            "OrderPublisher initialized. Will connect to RabbitMQ on first publish. Host: {HostName}:{Port}",
            hostName,
            port
        );

        // Configure retry policy: 3 retries with exponential backoff (1s, 2s, 4s)
        _retryPolicy = Policy
            .Handle<Exception>() // Retry on any exception
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)), // 1s, 2s, 4s
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "RabbitMQ publish failed. Retry {RetryCount}/3 after {Delay}ms",
                        retryCount,
                        timeSpan.TotalMilliseconds
                    );
                }
            );
    }

    private void EnsureConnected()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        lock (_lock)
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            try
            {
                // Close existing connection if it exists but is not open
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();

                // Create new connection and channel
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare exchange (creates it if it doesn't exist)
                // Durable = true means exchange survives server restart
                _channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: _exchangeType,
                    durable: true,
                    autoDelete: false
                );

                _logger.LogInformation(
                    "RabbitMQ connection established. Exchange: {ExchangeName}, Type: {ExchangeType}",
                    _exchangeName,
                    _exchangeType
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection");
                throw;
            }
        }
    }

    public async Task PublishOrderAsync(OrderDTO order, CancellationToken cancellationToken = default)
    {
        /*
         * Execute publish operation with retry policy.
         * If RabbitMQ is temporarily unavailable, Polly will automatically retry.
         */
        await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                // Ensure connection is established (lazy connection)
                EnsureConnected();

                // Serialize OrderDTO to JSON
                var json = JsonSerializer.Serialize(order, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var body = Encoding.UTF8.GetBytes(json);

                // Create message properties
                var properties = _channel!.CreateBasicProperties();
                properties.Persistent = true; // Message survives server restart
                properties.ContentType = "application/json";
                properties.MessageId = order.OrderId;
                properties.Timestamp = new AmqpTimestamp(
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );

                // Publish message to exchange
                // For fanout exchange, routing key is ignored but we include it anyway
                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: _routingKey,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation(
                    "Order published to RabbitMQ successfully. OrderId: {OrderId}, Exchange: {ExchangeName}",
                    order.OrderId,
                    _exchangeName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing order to RabbitMQ. OrderId: {OrderId}", order.OrderId);
                throw; // Re-throw so Polly can handle retry
            }
        });
    }

    public void Dispose()
    {
        if (!_disposed)
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

