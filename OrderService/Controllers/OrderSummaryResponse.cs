using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers
{
    public class OrderSummaryResponse
    {
        public string OrderId { get; set; } = default!;
        public string CustomerName { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public decimal ShippingCost { get; set; }

        // New fields
        public DateTime OrderDate { get; set; }
        public int NumberOfItems { get; set; }
    }
}
   