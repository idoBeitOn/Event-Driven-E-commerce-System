using SharedDTOs;

namespace OrderService.Services
{
	public class ProcessedOrdersStore
	{
		private readonly List<OrderDTO> _orders = new();//a list that holds all processed orders in memory.
        private readonly object _lock = new();//an object used to synchronize access to _orders across multiple threads.
											  //This prevents race conditions if multiple threads add or read orders at the same time.


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

		public OrderDTO? GetOrderById(string id)
		{
			lock (_lock)
				return _orders.FirstOrDefault(o => (o.OrderId) == id);
		}
	}
}
