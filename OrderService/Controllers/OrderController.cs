using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Data.Entities;
using Serilog.Filters;
using SharedDTOs;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        // DbContext is injected per request (scoped) by the DI container.
        // We use it to query the Orders and OrderItems tables.
        private readonly OrderDbContext _db;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderDbContext db, ILogger<OrdersController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /*
         * GET /api/orders/{id}
         * 
         * Flow:
         * 1) Query the Orders table by primary key.
         * 2) AsNoTracking() because this is read-only (faster, less memory).
         * 3) If not found → return 404.
         * 4) Map entity → response DTO (keeps API contract stable).
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderSummary(string id)
        {
            var order = await _db.Orders
                .AsNoTracking() // read-only query; no change tracking needed
                .Include(o => o.Items) // eager-load items (handy if you want to expand the response later)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found in database", id);
                return NotFound($"Order {id} not found");
            }

            var summary = new OrderSummaryResponse
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount,
                ShippingCost = order.ShippingCost,
                CustomerName = order.CustomerName,

                // Include extra fields for interview/demo clarity
                OrderDate = order.CreatedAt,
                NumberOfItems = order.Items?.Count ?? order.ItemsNum
            };

            _logger.LogDebug("Fetching order summary for OrderId {Orderid}",id);
            return Ok(summary);
        }
    }
}