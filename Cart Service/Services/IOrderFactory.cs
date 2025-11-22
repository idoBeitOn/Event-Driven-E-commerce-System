using Cart_Service.Models;
using Cart_Service.Models.Orders;

namespace Cart_Service.Services;

/// <summary>
/// Interface for OrderFactory - follows Interface Segregation Principle
/// This allows for dependency injection and easier testing
/// </summary>
public interface IOrderFactory
{
    /// <summary>
    /// Creates a complete OrderDTO from a OrderRequestDTO
    /// Generates all random data internally
    /// </summary>
    OrderDTO CreateOrder(OrderRequestDTO request);
}
