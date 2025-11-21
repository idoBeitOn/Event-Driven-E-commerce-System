namespace Cart_Service.Models.Orders;

public sealed class OrderDTO
{
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = "new";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public CustomerDTO Customer { get; set; } = new ();
    public ICollection<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    public int itemsNum { get; init; }
    public TotalsDTO Totals { get; init; } = new ();
    public PaymentDTO Payment { get; init; } = new ();
    public ShippingDTO Shipping { get; init; } = new ();
    public MetadataDTO Metadata { get; init; } = new ();
    
}