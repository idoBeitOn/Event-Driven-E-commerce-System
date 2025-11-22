using Cart_Service.Models;
using Cart_Service.Models.Orders;
using Cart_Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cart_Service.Controllers;

/// <summary>
/// Cart Controller - Handles HTTP requests for order creation
/// Follows Single Responsibility Principle: Only handles HTTP concerns
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly IOrderFactory _orderFactory;
    private readonly ILogger<CartController> _logger;

    /// <summary>
    /// Constructor - Dependency Injection provides IOrderFactory
    /// </summary>
    public CartController(IOrderFactory orderFactory, ILogger<CartController> logger)
    {
        _orderFactory = orderFactory;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order
    /// POST /api/cart/create-order
    /// </summary>
    [HttpPost("create-order")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult CreateOrder([FromBody] OrderRequestDTO request)
    {
        try
        {
            // Validate request
            if (!request.IsValid(out var error))
            {
                _logger.LogWarning("Invalid order request: {Error}", error);
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Generate complete order using factory pattern
            var order = _orderFactory.CreateOrder(request);

            _logger.LogInformation("Order created successfully: {OrderId}", order.OrderId);

            // Note: In next step, we'll publish this to RabbitMQ
            // For now, we just return the created order
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order: {OrderId}", request.OrderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while creating the order",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}

