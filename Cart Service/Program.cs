using Cart_Service.Services;
using SharedDTOs;

/*
Creates an object of type WebApplicationBuilder.
WebApplication is a class provided by ASP.NET Core that helps configure entire web application before it starts running.
*/
var builder = WebApplication.CreateBuilder(args);

/*
The Dependency Injection Container is a built-in framework component in ASP.NET Core that:
 Creates objects for you
 Manages their lifetime
 Injects them into your classes automatically
 Keeps your code loosely-coupled and testable
 */

/*
builder.services: Anything registered here becomes available for injection in controllers, services, etc.
Add services to the container.
AddScoped registers a service with a scoped lifetime.
A new instance of OrderFactory will be created once per HTTP request.
*/
builder.Services.AddScoped<IOrderFactory, OrderFactory>();


/*
Registers a service as a singleton, meaning only one instance of OrderPublisher is ever created.
RabbitMQ connections are expensive to create and are intended to be reused.
All uses of IOrderFactory within the same request will get the same instance.
On the next HTTP request, a new instance will be created.
If you created a new OrderPublisher (and connection) for every request, it would:
Consume unnecessary resources
Reduce performance
Risk exhausting connection limits
RabbitMQ does not like many short connections
A connection = expensive (TCP handshake)
A channel is light, but also shouldn’t be recreated constantly
Publishing should be fast → reuse connection & channel
*/
builder.Services.AddSingleton<IOrderPublisher, OrderPublisher>();

/* 
This registers all your controller classes (like CartController) with the DI container and the ASP.NET Core routing system. 
Lets ASP.NET Core discover all controllers in your project.
Configures the framework to map HTTP requests to controller actions (methods like CreateOrder).
Integrates features like:
Model binding ([FromBody], [FromQuery])
Validation ([ProducesResponseType])
Filters (like [ApiController])
when a request comes to /api/cart/create-order, ASP.NET Core knows which method to call.


1. Routing:
    Determines which controller and method handles a specific HTTP request URL.

2. Model Binding:
     Automatically converts HTTP request data (JSON body, query string, route parameters) into C# objects.

3. Validation:
      Checks that incoming request data meets rules (e.g., required fields, data formats) before it reaches your business logic.

4. Dependency Injection (DI):
      Automatically provides the required services or objects to a class, instead of creating them manually.
*/

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//make it easy to view, test, and share your API with other developers or teams.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*
 * Health Checks - Production-ready monitoring endpoint
 * 
 * Simple health check that returns OK if the service is running.
 * Useful for load balancers and orchestrators to verify service availability.
 */
builder.Services.AddHealthChecks();

/*          
Build() returns a WebApplication instance that represents the fully configured app.  
app now holds the running web application object.
 */
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

/*
 * Health check endpoint
 * GET /health → Returns 200 OK if service is healthy
 */
app.MapHealthChecks("/health");

app.Run();
