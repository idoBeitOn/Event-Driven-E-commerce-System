using OrderService.Logic;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

/*
 * Registers ProcessedOrdersStore as a singleton in the dependency injection container.
 * Only one instance of this store will exist for the lifetime of the app.
 * This makes sense because you want all consumers and controllers to see the same in-memory orders.

 */
builder.Services.AddSingleton<ProcessedOrdersStore>();


builder.Services.AddSingleton<OrderConsumer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var store = sp.GetRequiredService<ProcessedOrdersStore>();
    var logger = sp.GetRequiredService<ILogger<OrderConsumer>>();

    var rabbitConfig = config.GetSection("RabbitMQ");

    string hostName = rabbitConfig["HostName"] ?? "rabbitmq"; // must match docker service name
    int port = int.Parse(rabbitConfig["Port"] ?? "5672");
    string user = rabbitConfig["UserName"] ?? "guest";
    string pass = rabbitConfig["Password"] ?? "guest";
    string queue = rabbitConfig["QueueName"] ?? "order-queue";
    string exchange = rabbitConfig["ExchangeName"] ?? "order-exchange";
    return new OrderConsumer(hostName, port, user, pass, queue ,exchange, store, logger);
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
