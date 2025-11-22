namespace Cart_Service.Models.Orders;

public sealed class PaymentDTO
{
    public string Method { get; init; } = string.Empty;
    public string TransactionId { get; init; } = string.Empty;
    public bool Paid { get; init; }
}

