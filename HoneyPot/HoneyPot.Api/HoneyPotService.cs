using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class HoneyPotService : IHostedService
    {
        private readonly HoneyCollector _collector;
        private readonly HoneyPotServiceOptions _options;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public HoneyPotService(
            IOptionsMonitor<HoneyPotServiceOptions> options,
            HoneyCollector collector,
            IDistributedCache cache,
            ILogger<HoneyPotService> logger)
        {
            _options = options.CurrentValue;
            _collector = collector;
            _cache = cache;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Polling interval is: {_options.PollingIntervalInMinutes} minutes.", Array.Empty<object>());

                await Task.Delay(TimeSpan.FromMinutes(_options.PollingIntervalInMinutes));

                if (_collector.Count == 0)
                    continue;

                try
                {
                    foreach (var collectedHoney in _collector)
                    {
                        var name = collectedHoney.Key;
                        var total = collectedHoney.Value;

                        _logger.LogInformation($"Collection honey for: {name}", Array.Empty<object>());

                        var item = await _cache.GetAsync(name, cancellationToken);

                        if (item == null)
                        {
                            var payload = JsonSerializer.Serialize(new HoneyAmount { TotalAmount = total });
                            await _cache.SetStringAsync(name, payload, cancellationToken);
                        }
                        else
                        {
                            using var stream = new MemoryStream(item);
                            var honeyAmount = await JsonSerializer.DeserializeAsync<HoneyAmount>(stream, cancellationToken: cancellationToken);

                            honeyAmount.TotalAmount += total;

                            var payload = JsonSerializer.Serialize(honeyAmount);
                            await _cache.SetStringAsync(name, payload, cancellationToken);
                        }
                        _logger.LogWarning($"Name: {name}. Total time: {total}.", Array.Empty<object>());
                    }

                    _logger.LogWarning($"Processed {_collector.Count} notifications.", Array.Empty<object>());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Caching honey failed.", Array.Empty<object>());
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
