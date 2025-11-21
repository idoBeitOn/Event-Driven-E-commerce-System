namespace Cart_Service.Models.Orders;

public sealed class MetadataDto
{
    public string Source { get; init; } = "cart-service";
    public int Version { get; init; } = 1;
}

