namespace Cart_Service.Models.Orders;

public sealed class TotalsDTO
{
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal Discount { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
}

