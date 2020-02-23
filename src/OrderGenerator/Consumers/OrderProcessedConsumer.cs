using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceModel.Events;

namespace SagasDemo.OrderGenerator.Consumers
{
    public class OrderProcessedConsumer : IConsumer<IOrderProcessed>
    {
        private readonly ILogger<OrderProcessedConsumer> _logger;

        public OrderProcessedConsumer(ILogger<OrderProcessedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrderProcessed> context)
        {
            _logger.LogInformation($"Order processed to {context.Message.CorrelationId} was received");
        }
    }
}
