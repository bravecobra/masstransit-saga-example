using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceModel.Commands;
using ServiceModel.DTOs;
using ServiceModel.Events;

namespace Dispatcher.Consumers
{
    public class ShipOrderConsumer : IConsumer<IShipOrder>
    {
        private readonly ILogger<ShipOrderConsumer> _logger;

        public ShipOrderConsumer(ILogger<ShipOrderConsumer> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IShipOrder> context)
        {
            _logger.LogInformation($"Shipt order received {context.Message.CorrelationId}");
            await Task.Delay(2000);
            this.UpdateOrderState(context.Message.Order);
            await context.Publish<IOrderShipped>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });
        }

        private void UpdateOrderState(Order order) =>
           order.Status = Status.Shipped;
    }
}
