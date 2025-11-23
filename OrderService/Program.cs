using OrderService.Logic;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<ProcessedOrdersStore>();

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

// Register the hosted service
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
