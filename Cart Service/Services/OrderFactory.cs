
using SharedDTOs;
namespace Cart_Service.Services;

/*
This class is an implementation of the Factory Design Pattern.
Its purpose is:
✔ To generate a fully-populated OrderDTO
✔ To encapsulate all the random data logic
✔ To keep your controller clean and readable
✔ To centralize order-creation logic in one place
*/

public class OrderFactory : IOrderFactory
{
    private readonly Random _random = new();

    // Sample data pools for random generation
    private readonly string[] _firstNames = { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Jessica" };
    private readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
    private readonly string[] _cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego" };
    private readonly string[] _countries = { "USA", "Canada", "UK", "Germany", "France", "Australia" };
    private readonly string[] _productNames = { "Laptop", "Smartphone", "Headphones", "Tablet", "Monitor", "Keyboard", "Mouse", "Webcam", "Speaker", "Charger" };
    private readonly string[] _categories = { "Electronics", "Computers", "Accessories", "Audio", "Mobile" };
    private readonly string[] _paymentMethods = { "credit_card", "debit_card", "paypal", "apple_pay", "google_pay" };
    private readonly string[] _shippingMethods = { "standard", "express", "overnight", "priority" };

    public OrderDTO CreateOrder(OrderRequestDTO request)
    {
        // Generate customer data
        var customer = GenerateCustomer();

        // Generate order items
        var items = GenerateOrderItems(request.ItemsNum);

        // Calculate totals
        var totals = CalculateTotals(items);

        // Generate payment data
        var payment = GeneratePayment();

        // Generate shipping data
        var shipping = GenerateShipping(customer);

        // Assemble the complete order
        return new OrderDTO
        {
            OrderId = request.OrderId,
            Status = "new",
            CreatedAt = DateTime.UtcNow,
            Customer = customer,
            Items = items,
            itemsNum = request.ItemsNum,
            Totals = totals,
            Payment = payment,
            Shipping = shipping,
            Metadata = new MetadataDTO
            {
                Source = "cart-service",
                Version = 1
            }
        };
    }

    private CustomerDTO GenerateCustomer()
    {
        var firstName = _firstNames[_random.Next(_firstNames.Length)];
        var lastName = _lastNames[_random.Next(_lastNames.Length)];
        var city = _cities[_random.Next(_cities.Length)];
        var country = _countries[_random.Next(_countries.Length)];

        return new CustomerDTO
        {
            //string interpolation — inserting variables inside a string.
            //More readable
            //No messy string concatenation
            CustomerId = $"CUST-{_random.Next(10000, 99999)}",
            Name = $"{firstName} {lastName}",
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
            Address = $"{_random.Next(100, 9999)} Main Street",
            City = city,
            Country = country,
            PostalCode = _random.Next(10000, 99999).ToString()
        };
    }

    private List<OrderItemDTO> GenerateOrderItems(int itemsNum)
    {
        var items = new List<OrderItemDTO>();

        for (int i = 0; i < itemsNum; i++)
        {
            var productName = _productNames[_random.Next(_productNames.Length)];
            var category = _categories[_random.Next(_categories.Length)];
            var quantity = _random.Next(1, 5); // 1-4 items per line
            //NextDoubleGenerates random number between 0.0 and 1.0
            var unitPrice = (decimal)(_random.NextDouble() * 500 + 10); // $10-$510
            unitPrice = Math.Round(unitPrice, 2);

            items.Add(new OrderItemDTO
            {
                LineId = $"LINE-{i + 1:D3}",
                Name = productName,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Currency = "USD",
                Category = category
            });
        }

        return items;
    }

    private TotalsDTO CalculateTotals(List<OrderItemDTO> items)
    {
        //LINQ = Language Integrated Query — a set of methods for working with collections.
        //For each item, multiply unit price by quantity.
        var subTotal = items.Sum(item => item.UnitPrice * item.Quantity);
        var taxRate = 0.08m; // 8% tax
        var tax = Math.Round(subTotal * taxRate, 2);
        var discount = 0m; // No discount for now
        var totalAmount = Math.Round(subTotal + tax - discount, 2);

        return new TotalsDTO
        {
            SubTotal = (double)subTotal,
            Tax = (double)tax,
            Discount = (double)discount,
            TotalAmount = (double)totalAmount,
            Currency = "USD"
        };
    }

    private PaymentDTO GeneratePayment()
    {
        return new PaymentDTO
        {
            Method = _paymentMethods[_random.Next(_paymentMethods.Length)],

            /*
             Guid.NewGuid() → creates a 128-bit unique ID
            .ToString("N") → converts it to a simple string without dashes
            .Substring(0, 12) → take first 12 chars
            .ToUpper() → capitalize "TXN-{...}" → prefix with TXN-
             Example output: TXN-A3F9B42C197D
            */
            TransactionId = $"TXN-{Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}",
            Paid = false // Orders start as unpaid
        };
    }

    private ShippingDTO GenerateShipping(CustomerDTO customer)
    {
        var shippingMethod = _shippingMethods[_random.Next(_shippingMethods.Length)];

        //Switch expression - More concise,  Returns a value, Easier to read  
        
        var estimatedDays = shippingMethod switch
        {
            "overnight" => 1,
            "express" => 2,
            "priority" => 3,
            _ => 5 // standard
        };

        return new ShippingDTO
        {
            Method = shippingMethod,
            Address = customer.Address,
            City = customer.City,
            Country = customer.Country,
            PostalCode = customer.PostalCode,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(estimatedDays)
        };
    }
}

