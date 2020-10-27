using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class HoneyPotService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly Queue<HoneySentNotification> _notifications;
        private readonly HoneyPotServiceOptions _options;

        public HoneyPotService(IOptionsMonitor<HoneyPotServiceOptions> options, ILogger<HoneyPotService> logger, Queue<HoneySentNotification> notifications)
        {
            _options = options.CurrentValue;
            _logger = logger;
            _notifications = notifications;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Polling interval is: {_options.PollingIntervalInMinutes} minutes.", Array.Empty<object>());

                await Task.Delay(TimeSpan.FromMinutes(_options.PollingIntervalInMinutes));

                var notificationGroups = _notifications.Take(_notifications.Count).GroupBy(x => x.Name);

                foreach(var group in notificationGroups)
                {
                    double totalTime = group.Sum(x => x.TimeTook);

                    _logger.LogWarning($"Name: {group.Key}. Total time: {totalTime}. Requests count: {group.Count()}", Array.Empty<object>());
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
