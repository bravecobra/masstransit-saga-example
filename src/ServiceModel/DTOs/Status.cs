using System;

namespace ServiceModel.DTOs
{
    [Serializable]
    public enum Status
    {
        StockReserved,
        Submitted,
        Payed,
        Shipped, 
        Processed,
    }
}
