using System.Threading.Tasks;

namespace Bus.Abstractions
{
    public interface IIntegrationBus
    {
        Task Publish<T>(T message) where T : class, IEvent;
    }
}
