using System;
using System.Collections.Generic;

namespace ServiceModel.DTOs
{
    [Serializable]
    public class ShoppingCart
    {
        public List<Product> Products { get; set; }
    }
}
