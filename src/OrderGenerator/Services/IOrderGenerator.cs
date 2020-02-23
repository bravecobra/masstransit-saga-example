using ServiceModel.DTOs;

namespace SagasDemo.OrderGenerator.Services
{
    public interface IOrderGenerator
    {
        Order Generate();
    }
}
