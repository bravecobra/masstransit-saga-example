using System;
using ServiceModel.DTOs;
using ServiceModel.Events;

namespace OrderProcessor.Events
{
    [Serializable]
    public class OrderProcessed : IOrderProcessed
    {
        public OrderProcessed(Guid correlationId, Order order)
        {
            CorrelationId = correlationId;
            Order = order;
        }
        public Guid CorrelationId { get; }

        public Order Order { get; }
    }
}
