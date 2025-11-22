namespace SharedDTOs;
public sealed class OrderItemDTO
{
    public string LineId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public string Category { get; init; } = string.Empty;
}



