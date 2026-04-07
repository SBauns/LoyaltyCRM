using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PapasCRM_API.Services.Interfaces
{
    public interface IYearcardCleanupService
    {
        public Task StartAsync(CancellationToken cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken);

        void Dispose();
    }
}