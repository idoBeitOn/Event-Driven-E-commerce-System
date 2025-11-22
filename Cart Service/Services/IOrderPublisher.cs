using Cart_Service.Models.Orders;

namespace Cart_Service.Services;

/// <summary>
/// Interface for OrderPublisher - follows Interface Segregation Principle
/// Abstracts the RabbitMQ publishing logic
/// </summary>
public interface IOrderPublisher
{
    /// <summary>
    /// Publishes an order event to RabbitMQ
    /// Serializes the order to JSON and broadcasts it to all consumers
    /// </summary>
    Task PublishOrderAsync(OrderDTO order, CancellationToken cancellationToken = default);
}

