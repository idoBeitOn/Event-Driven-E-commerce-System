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

        //This receives the OrderConsumer instance from dependency injection
        //This is the consumer that listens to RabbitMQ.
        public OrderConsumerHostedService(OrderConsumer orderConsumer)
		{
			_orderConsumer = orderConsumer;
		}


        /*
		 * the service immediately starts listening to RabbitMQ
		 * it does NOT block the web server
		 * your OrderService can still handle HTTP GET requests
		 */
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// Start consuming messages in the background
			_orderConsumer.StartConsuming(stoppingToken);
			return Task.CompletedTask;
		}

		public override void Dispose()
		{
			_orderConsumer.Dispose();
			base.Dispose();
		}
	}
}
