using SharedDTOs;

namespace OrderService.Services
{
	public class ProcessedOrdersStore
	{
		private readonly List<OrderDTO> _orders = new();
		private readonly object _lock = new();

		public void AddOrder(OrderDTO order)
		{
			lock (_lock)
				_orders.Add(order);
		}

		public IEnumerable<OrderDTO> GetOrders()
		{
			lock (_lock)
				return _orders.ToList(); // return a copy
		}

		public OrderDTO? GetOrderById(Guid id)
		{
			lock (_lock)
				return _orders.FirstOrDefault(o => Guid.Parse(o.OrderId) == id);
		}
	}
}
