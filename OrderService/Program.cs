using OrderService.Logic;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// Register your OrderConsumer as singleton (we want one instance for the queue)
builder.Services.AddSingleton<OrderConsumer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<OrderConsumer>>();
    return new OrderConsumer("localhost", "order-queue", logger);
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
