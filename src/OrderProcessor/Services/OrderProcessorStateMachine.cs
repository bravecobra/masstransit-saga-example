using System;
using System.Threading.Tasks;
using Automatonymous;
using Automatonymous.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderProcessor.Events;
using OrderProcessor.State;
using ServiceModel;
using ServiceModel.Commands;
using ServiceModel.DTOs;
using ServiceModel.Events;
using SagaState = Automatonymous.State;
namespace OrderProcessor.Services
{
    public sealed class OrderProcessorStateMachine : MassTransitStateMachine<ProcessingOrderState>
    {
        private readonly ILogger<OrderProcessorStateMachine> _logger;
        private readonly RabbitMqOptions _rabbitmqOptions;

        public OrderProcessorStateMachine(ILogger<OrderProcessorStateMachine> logger, IOptions<RabbitMqOptions> rabbitmqOptions)
        {
            _logger = logger;
            _rabbitmqOptions = rabbitmqOptions.Value;
            InstanceState(x => x.State);
            State(() => Processing);
            ConfigureCorrelationIds();
            Initially(SetOrderSummitedHandler());
            During(Processing, SetStockReservedHandler(), SetPaymentProcessedHandler(), SetOrderShippedHandler());
            SetCompletedWhenFinalized();
        }

        private void ConfigureCorrelationIds()
        {
            Event(() => OrderSubmitted, x => x
                .CorrelateById(c => c.Message.CorrelationId)
                .SelectId(c => c.Message.CorrelationId));
            Event(() => StockReserved, x => x
                .CorrelateById(c => c.Message.CorrelationId));
            Event(() => PaymentProcessed, x => x
                .CorrelateById(c => c.Message.CorrelationId));
            Event(() => OrderShipped, x => x
                .CorrelateById(c => c.Message.CorrelationId));
        }

        private EventActivityBinder<ProcessingOrderState, IOrderSubmitted> SetOrderSummitedHandler() =>
            When(OrderSubmitted).Then(c => UpdateSagaState(c.Instance, c.Data.Order))
                                .Then(c => _logger.LogInformation($"Order submitted to {c.Data.CorrelationId} received"))
                                .ThenAsync(c => SendCommand<IReserveStock>(_rabbitmqOptions.WarehouseQueue, c))
                                .TransitionTo(Processing);


        private EventActivityBinder<ProcessingOrderState, IStockReserved> SetStockReservedHandler() =>
            When(StockReserved).Then(c => UpdateSagaState(c.Instance, c.Data.Order))
                               .Then(c => _logger.LogInformation($"Stock reserved to {c.Data.CorrelationId} received"))
                               .ThenAsync(c => SendCommand<IProcessPayment>(_rabbitmqOptions.CashierQueue, c));


        private EventActivityBinder<ProcessingOrderState, IPaymentProcessed> SetPaymentProcessedHandler() =>
            When(PaymentProcessed).Then(c => UpdateSagaState(c.Instance, c.Data.Order))
                                  .Then(c => _logger.LogInformation($"Payment processed to {c.Data.CorrelationId} received"))
                                  .ThenAsync(c => SendCommand<IShipOrder>(_rabbitmqOptions.DispatcherQueue, c));


        private EventActivityBinder<ProcessingOrderState, IOrderShipped> SetOrderShippedHandler() =>
            When(OrderShipped).Then(c =>
                                        {
                                            UpdateSagaState(c.Instance, c.Data.Order);
                                            c.Instance.Order.Status = Status.Processed;
                                        })
                              .Publish(c => new OrderProcessed(c.Data.CorrelationId, c.Data.Order))
                              .Finalize();

        private void UpdateSagaState(ProcessingOrderState state, Order order)
        {
            var currentDate = DateTime.Now;
            state.Created = currentDate;
            state.Updated = currentDate;
            state.Order = order;
        }

        private async Task SendCommand<TCommand>(string endpointKey, BehaviorContext<ProcessingOrderState, IMessage> context)
            where TCommand : class, IMessage
        {
            var sendEndpoint = await context.GetSendEndpoint(new Uri(endpointKey));
            await sendEndpoint.Send<TCommand>(new
            {
                context.Data.CorrelationId,
                context.Data.Order
            });
        }
        public SagaState Processing { get; private set; }
        public Event<IOrderSubmitted> OrderSubmitted { get; private set; }
        public Event<IOrderShipped> OrderShipped { get; set; }
        public Event<IPaymentProcessed> PaymentProcessed { get; private set; }
        public Event<IStockReserved> StockReserved { get; private set; }
    }
}
