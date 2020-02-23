using System;

namespace ServiceModel.DTOs
{
    [Serializable]
    public enum Status
    {
        StockReserved,
        Submitted,
        Paymented,
        Shipped, 
        Processed,
    }
}
