using Microsoft.AspNetCore.Mvc;
using OrderService.Services;
using SharedDTOs;
namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ProcessedOrdersStore _store;

        public OrdersController(ProcessedOrdersStore store)
        {
            _store = store;
        }

        [HttpGet("{id}")]
        public IActionResult GetOrderSummary(string id)
        {
            var order = _store.GetOrderById(id);
            if (order == null)
                return NotFound($"Order {id} not found");

            var summary = new OrderSummaryResponse
            {
                OrderId = order.OrderId,
                TotalAmount = (decimal)order.Totals.TotalAmount,
                ShippingCost = (decimal)order.Shipping.ShippingCost,
                CustomerName = order.Customer?.Name ?? "Unknown"
            };

            return Ok(summary);
        }
    }

}


