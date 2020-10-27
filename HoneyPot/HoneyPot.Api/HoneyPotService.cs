using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        public HoneyPotService(ILogger<HoneyPotService> logger, Queue<HoneySentNotification> notifications)
        {
            _logger = logger;
            _notifications = notifications;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while(true)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));

                var notificationGroups = _notifications.Take(100).GroupBy(x => x.Name);

                foreach(var group in notificationGroups)
                {
                    double totalTime = group.Sum(x => x.TimeTook);

                    _logger.LogWarning($"Name: {group.Key}. Total time: {totalTime}", Array.Empty<object>());
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
