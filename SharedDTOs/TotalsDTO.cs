namespace SharedDTOs;
public sealed class TotalsDTO
{
    public double SubTotal { get; init; }
    public double Tax { get; init; }
    public double Discount { get; init; }
    public double TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
}

