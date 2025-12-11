namespace OrderService.Data.Entities;

/*
 * Entity Class - Represents a row in the "OrderItems" table
 * 
 * This is a "child" entity - it belongs to an Order.
 * The relationship is: One Order → Many OrderItems
 * 
 * Foreign Key: OrderId (links back to Order.OrderId)
 */

public class OrderItem
{
    // Primary Key - Auto-incrementing ID (1, 2, 3, ...)
    public int Id { get; set; }

    // Foreign Key - Links to Order.OrderId
    public string OrderId { get; set; } = string.Empty;

    // Line Item Details
    public string LineId { get; set; } = string.Empty; // e.g., "LINE-001"
    public string Name { get; set; } = string.Empty; // Product name
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string Category { get; set; } = string.Empty;

    /*
     * Navigation Property (optional) - Links back to parent Order
     * 
     * Not required, but useful if you want to navigate from OrderItem → Order
     * Example: orderItem.Order.CustomerName
     */
    // public Order? Order { get; set; }
}
