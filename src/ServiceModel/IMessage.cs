using System;
using ServiceModel.DTOs;

namespace ServiceModel
{
    public interface IMessage
    {
        Guid CorrelationId { get;  }

        Order Order { get;  }


    }
}
