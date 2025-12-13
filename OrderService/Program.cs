using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Logic;
using OrderService.Services;
using Serilog;



var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/orders.log")
    .CreateLogger();

builder.Host.UseSerilog();


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
    /*
     * MigrationsAssembly: explicitly point EF Core to the assembly that contains migrations.
     * Without this, the runtime inside the container might not discover the migration class,
     */
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("OrdersDb"),
        npgsql => npgsql.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)
    ));

builder.Services.AddSingleton<OrderConsumer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    var logger = sp.GetRequiredService<ILogger<OrderConsumer>>();

    var rabbitConfig = config.GetSection("RabbitMQ");

    string hostName = rabbitConfig["HostName"] ?? "rabbitmq"; 
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

/*
 * Automatic Database Migration on Startup
 * 
 * This ensures the database schema is always up-to-date when the service starts.
 * 
 * How it works:
 * 1. Creates a scope (required to resolve scoped services like DbContext)
 * 2. Gets the OrderDbContext from DI
 * 3. Calls Database.Migrate() which:
 *    - Checks which migrations have been applied
 *    - Applies any pending migrations automatically
 *    - Creates the database if it doesn't exist
 * 
 * Why this is useful:
 * - No need to manually run "dotnet ef database update"
 * - Works automatically in Docker containers
 * - Ensures database is always in sync with code
 * 
 * Note: In production, you might want to run migrations separately as part of CI/CD,
 * but for development and containerized apps, this is a common pattern.
 */
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        // Log discovered migrations and pending migrations to help debug
        var allMigrations = dbContext.Database.GetMigrations().ToList();
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        logger.LogInformation("Discovered migrations: {Count} -> {Migrations}", allMigrations.Count, string.Join(", ", allMigrations));
        logger.LogInformation("Pending migrations: {Count} -> {Migrations}", pendingMigrations.Count, string.Join(", ", pendingMigrations));
        logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }

    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw; // Fail fast if migrations can't be applied
    }

}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
Log.Information("OrderService is starting...");


try
{
    app.Run();
}

finally
{
    Log.CloseAndFlush();
}

