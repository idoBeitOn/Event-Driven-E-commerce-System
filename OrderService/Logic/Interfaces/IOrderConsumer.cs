using SharedDTOs;

namespace OrderService.Logic.Interfaces
{
    //Probably should be deleted!!!
    //Abstracts the RabbitMQ consuming logic.

    //De-serialize JSON to OrderDTO type variable in order to make calculations.
    public interface IOrderConsumer : IDisposable
    {
        //Task StartAsync(CancellationToken cancellationToken = default);
        //Task StopAsync(OrderDTO orderDTO, CancellationToken cancellationToken = default);
        //Task ConsumeOrderAsync(OrderDTO orderDTO, CancellationToken cancelToken = default);
    }
}
