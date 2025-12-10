using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Logic;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

/*
 * Register Entity Framework Core DbContext
 * 
 * AddDbContext registers OrderDbContext with the dependency injection container.
 * 
 * Scoped Lifetime:
 * - A new DbContext instance is created for each HTTP request
 * - This is important because DbContext is NOT thread-safe
 * - After the request completes, the DbContext is disposed
 * 
 * What happens:
 * 1. EF Core reads the connection string from appsettings.json (ConnectionStrings:OrdersDb)
 * 2. Creates a DbContextOptions object with that connection string
 * 3. Registers OrderDbContext so it can be injected into controllers/services
 * 
 * Usage:
 * - In a controller: public OrdersController(OrderDbContext context) { ... }
 * - EF Core automatically provides the configured DbContext
 */
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));

builder.Services.AddSingleton<OrderConsumer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    var logger = sp.GetRequiredService<ILogger<OrderConsumer>>();

    var rabbitConfig = config.GetSection("RabbitMQ");

    string hostName = rabbitConfig["HostName"] ?? "rabbitmq"; // must match docker service name
    int port = int.Parse(rabbitConfig["Port"] ?? "5672");
    string user = rabbitConfig["UserName"] ?? "guest";
    string pass = rabbitConfig["Password"] ?? "guest";
    string queue = rabbitConfig["QueueName"] ?? "order-queue";
    string exchange = rabbitConfig["ExchangeName"] ?? "order-exchange";
    return new OrderConsumer(hostName, port, user, pass, queue ,exchange, scopeFactory, logger);
});



builder.Services.AddHostedService<OrderConsumerHostedService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
