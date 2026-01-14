using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedDTOs;
using RabbitMQConsume;
using OrderService.Logic;

namespace OrderService.Services
{
    /*
	 * it’s a background service in ASP.NET Core.
	 * A BackgroundService is something that:
	 * starts automatically when your app starts
	 * runs in the background (not tied to HTTP requests)
	 * shuts down gracefully when the app stops
	 * Perfect for consumers like RabbitMQ listeners.
	 */
    public class OrderConsumerHostedService : BackgroundService
	{
		private readonly OrderConsumer _orderConsumer;
		private readonly ILogger<OrderConsumerHostedService> _logger;

        //This receives the OrderConsumer instance from dependency injection
        //This is the consumer that listens to RabbitMQ.
        public OrderConsumerHostedService(OrderConsumer orderConsumer, ILogger<OrderConsumerHostedService> logger)
        {
            _orderConsumer = orderConsumer;
            _logger = logger;
        }


        /*
		 * the service immediately starts listening to RabbitMQ
		 * it does NOT block the web server
		 * your OrderService can still handle HTTP GET requests
		 */
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("OrderConsumerHostedService starting");
            stoppingToken.Register(() => _logger.LogInformation("OrderConsumerHostedService stopping (cancellation requested)"));
            // Start consuming messages in the background
            _orderConsumer.StartConsuming(stoppingToken);
			return Task.CompletedTask;
		}

		public override void Dispose()
		{
            _logger.LogInformation("OrderConsumerHostedService disposing");
            _orderConsumer.Dispose();
			base.Dispose();
		}
	}
}
