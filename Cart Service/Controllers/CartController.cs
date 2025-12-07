
/*
- A framework is a collection of libraries, tools, and pre-built functionality that helps
developers build applications without starting from scratch.

- ASP.NET = Active Server Pages.NET. It is a framework for building: 
backend web applications, REST API's, web services.

- MVC = Model–View–Controller
It is an architectural pattern used to structure applications:
Model → data + business logic
View → UI (not really used in Web APIs)
Controller → handles HTTP requests (what CartController does)
 */

using Microsoft.AspNetCore.Mvc;
using SharedDTOs;
using Cart_Service.Services;
namespace CartService.Controllers;

/*
A Controller is a class that handles HTTP requests.
Cart Controller - Handles HTTP requests for order creation.
Attributes are metadata attached to classes/methods to change how they behave.
Examples:
[ApiController] → tells .NET this is an API controller
[Route("api/cart")] → defines the URL
[HttpPost] → marks a method as POST
[ProducesResponseType] → documents response types
*/
[ApiController]

//This is an attribute that defines the base URL path for all endpoints in the controller.
[Route("api/[controller]")]

//CartController → by convention, controllers in ASP.NET Core end with Controller.
//Convention is important because[controller] in routes automatically uses the class name minus "Controller".
public class CartController : ControllerBase
{
    /*
    readonly guarantees that these services cannot be replaced accidentally after the controller is created.
    It’s a best practice for dependency-injected services.
    Instead of creating the objects inside the class, we ask for them in a constructor.
    */
    private readonly IOrderFactory _orderFactory;
    private readonly IOrderPublisher _orderPublisher;
    private readonly ILogger<CartController> _logger;

    // Constructor - Dependency Injection provides IOrderFactory and IOrderPublisher
    public CartController(
        IOrderFactory orderFactory,//Used to create order objects
        IOrderPublisher orderPublisher,//used to publish orders to RabbitMQ.
        ILogger<CartController> logger)//logs messages and errors.
    {
        _orderFactory = orderFactory;
        _orderPublisher = orderPublisher;
        _logger = logger;
    }
    /*
       Creates a new order 
      "create-order" is appended to the base route
       Full route becomes: POST /api/cart/create-order
     */


    /*
      Marks this method as a POST endpoint.
      Appends "create-order" to the controller base route (/api/cart → /api/cart/create-order).
      
     */
    [HttpPost("create-order")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]


    /*
    When you await a task, the task is scheduled on a thread pool managed by the .NET runtime.
    The thread pool maintains a pool of worker threads that can be reused for various tasks, reducing the overhead of creating
    and destroying threads.
    Starting the Task: When you call an asynchronous method, it starts running on the main thread until it hits an await keyword.
    Awaiting the Task: Upon encountering an await, the method execution is paused, and control is returned to the calling method
    or the main thread. The awaited task is handed off to a thread pool thread.
    Thread Pool Execution: The task runs on a thread from the thread pool. If the task involves I/O operations
    (e.g., file access, web requests), the actual I/O work is handed off to the OS, and the thread is freed to perform other tasks
    while waiting for the I/O to complete.   
    Once the awaited task is completed, the continuation (the remaining code after the await) is scheduled back onto the original
    context, typically the main thread for UI applications, or a thread pool thread for server applications.

    Task Completion: When the background task is completed, the thread pool schedules the continuation of the async method.
    Resuming Execution: The async method resumes execution from where it left off after the await keyword, now with the result
    of the awaited task.
    */

    public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO request)
    {
        try
        {
            if (!request.IsValid(out var error))
            {
                _logger.LogWarning("Invalid order request: {Error}", error);
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            // Generate complete order using factory pattern
            var order = _orderFactory.CreateOrder(request);
            _logger.LogInformation("Order created successfully: {OrderId}", order.OrderId);

 
            try
            {
                /*
                 Asynchronously publish the order to RabbitMQ using the injected publisher.
                 await releases the thread while the I/O completes (improves scalability).
                 This is the point where the method defers to the messaging infrastructure.
                 */
                await _orderPublisher.PublishOrderAsync(order);
                _logger.LogInformation("Order published to RabbitMQ: {OrderId}", order.OrderId);
            }
            catch (Exception publishEx)
            {
                // Log error but don't fail the request - order was created successfully
                // In production, you might want to implement retry logic or outbox pattern
                _logger.LogError(publishEx, "Failed to publish order to RabbitMQ: {OrderId}", order.OrderId);
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order: {OrderId}", request.OrderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while creating the order",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}

