namespace Cart_Service.Models;

//DTO - Data Transfer Object - used to transfer data between different layers of the application
//Model for the request body of the CreateOrder endpoint
public sealed class OrderRequestDTO // Sealed class to prevent inheritance and ensure immutability
{
    public string OrderId { get; set; } = string.Empty;
    public int ItemsNum { get; set; }

    public bool IsValid(out string error) // out parameter to return error message
    {
        if (string.IsNullOrWhiteSpace(OrderId))
        {
            error = "orderId is required.";
            return false;
        }

        if (ItemsNum <= 0 || ItemsNum > 20)
        {
            error = "itemsNum must be between 1 and 20.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}


