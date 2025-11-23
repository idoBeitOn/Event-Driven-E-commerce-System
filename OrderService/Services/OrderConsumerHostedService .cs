using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedDTOs;
using RabbitMQConsume;
using OrderService.Logic;

namespace OrderService.Services
{
	public class OrderConsumerHostedService : BackgroundService
	{
		private readonly OrderConsumer _orderConsumer;

		public OrderConsumerHostedService(OrderConsumer orderConsumer)
		{
			_orderConsumer = orderConsumer;
		}

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
