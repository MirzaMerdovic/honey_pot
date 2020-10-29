using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class HoneyPotService : IHostedService
    {
        private readonly Queue<HoneySentRequest> _notifications;
        private readonly HoneyPotServiceOptions _options;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public HoneyPotService(
            IOptionsMonitor<HoneyPotServiceOptions> options,
            Queue<HoneySentRequest> notifications,
            IDistributedCache cache,
            ILogger<HoneyPotService> logger)
        {
            _options = options.CurrentValue;
            _notifications = notifications;
            _cache = cache;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Polling interval is: {_options.PollingIntervalInMinutes} minutes.", Array.Empty<object>());

                await Task.Delay(TimeSpan.FromMinutes(_options.PollingIntervalInMinutes));

                HoneySentRequest[] copy = new HoneySentRequest[_notifications.Count];
                _notifications.CopyTo(copy, 0);

                _notifications.Clear();

                try
                {
                    foreach (var group in copy.GroupBy(x => x.Name))
                    {
                        int totalAmount = group.Sum(x => x.Amount);

                        var item = await _cache.GetAsync(group.Key, cancellationToken);

                        if (item == null)
                        {
                            var payload = JsonSerializer.Serialize(new HoneyAmount { TotalAmount = totalAmount });
                            await _cache.SetStringAsync(group.Key, payload, cancellationToken);
                        }
                        else
                        {
                            using var stream = new MemoryStream(item);
                            var honeyAmount = await JsonSerializer.DeserializeAsync<HoneyAmount>(stream, cancellationToken: cancellationToken);

                            honeyAmount.TotalAmount += totalAmount;

                            var payload = JsonSerializer.Serialize(honeyAmount);
                            await _cache.SetStringAsync(group.Key, payload, cancellationToken);
                        }
                        _logger.LogWarning($"Name: {group.Key}. Total time: {totalAmount}. Requests count: {group.Count()}", Array.Empty<object>());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Caching honey failed.", Array.Empty<object>());

                    copy.ToList().ForEach(_notifications.Enqueue);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
