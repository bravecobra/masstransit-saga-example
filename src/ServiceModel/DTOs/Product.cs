using System;

namespace ServiceModel.DTOs
{
    [Serializable]
    public class Product
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
