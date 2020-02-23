using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceModel.Events;

namespace SagasDemo.OrderGenerator.Consumers
{
    public class OrderCancelledConsumer : IConsumer<IOrderCancelled>
    {
        private readonly ILogger<OrderCancelledConsumer> _logger;

        public OrderCancelledConsumer(ILogger<OrderCancelledConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrderCancelled> context)
        {
            this._logger.LogInformation($"The order cancelled to {context.Message.CorrelationId} was received");
        }
    }
}
