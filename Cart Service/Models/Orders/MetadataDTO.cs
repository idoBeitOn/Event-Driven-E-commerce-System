namespace Cart_Service.Models.Orders;

public sealed class MetadataDTO
{
    public string Source { get; init; } = "cart-service";
    public int Version { get; init; } = 1;
}

