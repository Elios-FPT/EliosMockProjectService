using MockProjectService.Core.Interfaces;

namespace MockProjectService.Infrastructure.Kafka
{
    public interface IKafkaConsumerFactory<T> where T : class
    {
        IKafkaConsumerRepository<T> CreateConsumer(string sourceServiceName);
    }
}