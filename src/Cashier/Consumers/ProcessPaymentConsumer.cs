using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceModel.Commands;
using ServiceModel.DTOs;
using ServiceModel.Events;

namespace Cashier.Consumers
{
    public class ProcessPaymentConsumer: IConsumer<IProcessPayment>
    {
        private readonly ILogger<ProcessPaymentConsumer> _logger;

        public ProcessPaymentConsumer(ILogger<ProcessPaymentConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IProcessPayment> context)
        {
            _logger.LogInformation($"Process payment command receives to {context.Message.CorrelationId}");
            await Task.Delay(2000);
            this.UpdateOrderState(context.Message.Order);
            await context.Publish<IPaymentProcessed>(new
            {
                CorrelationId = context.Message.CorrelationId, 
                Order = context.Message.Order
            });
            _logger.LogInformation($"Payment for customer {context.Message.Order.Customer.Name} processed to {context.Message.CorrelationId}");
        }

        private void UpdateOrderState(Order order) =>
            order.Status = Status.Paymented;
        
    }
}
