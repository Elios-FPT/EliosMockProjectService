using MockProjectService.Core.Models;

namespace MockProjectService.Core.Interfaces
{
    public interface IAppConfiguration
    {
        string GetKafkaBootstrapServers();
        string GetCurrentServiceName();
    }
}
