using System.Threading;
using System.Threading.Tasks;
using Bus.Abstractions;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public interface IOutbox<in T> where T : class, IMessage
    {
        Task AddAsync(T message, CancellationToken cancellationToken = default(CancellationToken));
        Task DispatchAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}