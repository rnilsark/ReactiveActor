using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Bus.MassTransit.ServiceFabric
{
    public class MassTransitCommunicationListener : ICommunicationListener
    {
        private readonly IBusControl _busControl;

        public MassTransitCommunicationListener(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            await _busControl.StartAsync(cancellationToken).ConfigureAwait(false);

            return _busControl.Address.AbsoluteUri;
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Abort()
        {
            _busControl.Stop();
        }
    }
}