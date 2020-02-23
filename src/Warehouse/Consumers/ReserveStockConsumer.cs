using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceModel.Commands;
using ServiceModel.DTOs;
using ServiceModel.Events;
// ReSharper disable RedundantAnonymousTypePropertyName

namespace Warehouse.Consumers
{
    public class ReserveStockConsumer : IConsumer<IReserveStock>
    {
        private readonly ILogger<ReserveStockConsumer> _logger;

        public ReserveStockConsumer(ILogger<ReserveStockConsumer> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IReserveStock> context)
        {
            _logger.LogInformation($"Reserve stock to {context.Message.CorrelationId} was received");
            await Task.Delay(2000);
            UpdateOrderState(context.Message.Order);
            await context.Publish<IStockReserved>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });
        }

        private void UpdateOrderState(Order order) => order.Status = Status.StockReserved;
    }
}
