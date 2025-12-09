namespace OrderService.Data.Entities;

/*
 * Entity Class - Represents a row in the "Orders" table
 * 
 * This is a C# class that EF Core will map to a database table.
 * Each property becomes a column in the table.
 * 
 * Key concepts:
 * - OrderId = Primary Key (unique identifier)
 * - Navigation Property (Items) = relationship to OrderItems
 * - Properties match the columns we designed in our schema
 */

public class Order
{
    // Primary Key - Unique identifier for each order
    public string OrderId { get; set; } = string.Empty;

    // Order Status
    public string Status { get; set; } = "new";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Customer Information (denormalized - stored directly in Orders table)
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerCountry { get; set; } = string.Empty;
    public string CustomerPostalCode { get; set; } = string.Empty;

    // Totals
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";

    // Shipping Information
    public string ShippingMethod { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; set; }

    // Payment Information
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public bool Paid { get; set; }

    // Metadata
    public string Source { get; set; } = "cart-service";
    public int Version { get; set; } = 1;

    // Items count (for quick reference)
    public int ItemsNum { get; set; }

    /*
     * Navigation Property - Represents the relationship to OrderItems
     * 
     * This is NOT stored as a column in the database.
     * EF Core uses this to load related OrderItems when you query an Order.
     * 
     * Example:
     *   var order = context.Orders.Include(o => o.Items).First();
     *   // Now order.Items contains all OrderItems for this order
     */
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
