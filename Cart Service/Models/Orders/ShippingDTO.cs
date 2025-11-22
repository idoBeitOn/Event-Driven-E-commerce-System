namespace Cart_Service.Models.Orders;

public sealed class ShippingDTO
{
    public string Method { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; init; }
}

