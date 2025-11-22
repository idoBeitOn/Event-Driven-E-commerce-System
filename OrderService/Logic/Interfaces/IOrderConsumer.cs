using SharedDTOs;

namespace OrderService.Logic.Interfaces
{

    //Abstracts the RabbitMQ consuming logic.
    //De-serialize JSON to OrderDTO type variable in order to make calculations.
    public interface IOrderConsumer
    {
        Task ConsumeOrderAsync(OrderDTO orderDTO, CancellationToken cancellationToken = default);
    }
}
