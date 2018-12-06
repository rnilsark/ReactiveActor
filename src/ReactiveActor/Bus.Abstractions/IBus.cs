using System.Threading.Tasks;

namespace Bus.Abstractions
{
    public interface IBus
    {
        Task Publish(IMessage message);
    }
}
