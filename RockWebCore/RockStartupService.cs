using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace RockWebCore
{
    public class RockStartupService : IHostedService
    {
        public async Task StartAsync( CancellationToken cancellationToken )
        {
            await Rock.Bus.RockMessageBus.StartAsync();
        }

        public Task StopAsync( CancellationToken cancellationToken )
        {
            return Task.CompletedTask;
        }
    }
}
