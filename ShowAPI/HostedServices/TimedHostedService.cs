using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using DataAccess.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ShowAPI.HostedServices
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> _logger;
        private readonly int _repeatTaskEveryHours;
        private readonly IServiceProvider _services;
        private Timer _timer;

        public TimedHostedService(ILogger<TimedHostedService> logger,
            IServiceProvider services, IOptionsMonitor<ScraperOptions> options)
        {
            _logger = logger;
            _services = services;
            _repeatTaskEveryHours = options.CurrentValue.RepeatTaskEveryHours;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed service running.");
            _timer = new Timer(async state => await RunAsync(cancellationToken), null, TimeSpan.Zero,
                TimeSpan.FromHours(_repeatTaskEveryHours));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed service stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                var scraperService = scope.ServiceProvider.GetService<IScopedService>();
                await scraperService.RunAsync(cancellationToken);
            }
        }
    }
}