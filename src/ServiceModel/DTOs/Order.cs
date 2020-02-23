using System;

namespace ServiceModel.DTOs
{
    [Serializable]
    public class Order
    {
        public Customer Customer { get; set; }
        public ShoppingCart Cart { get; set; }
        public Status Status { get; set; }
    }
}
