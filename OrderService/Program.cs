using OrderService.Logic;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<ProcessedOrdersStore>();


/*
var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
var hostName = rabbitConfig["HostName"];
var queueName = rabbitConfig["QueueName"];
//builder.Services.AddSingleton<OrderConsumer>(); // consumer itself
// Register your OrderConsumer as singleton (we want one instance for the queue)
builder.Services.AddSingleton<OrderConsumer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<OrderConsumer>>();
    var store = sp.GetRequiredService<ProcessedOrdersStore>();
    return new OrderConsumer(config, logger, store);
});
*/
// Register the hosted service



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
