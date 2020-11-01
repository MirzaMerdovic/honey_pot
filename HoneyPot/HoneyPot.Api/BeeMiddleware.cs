using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class BeeMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly RequestDelegate _next;
        private readonly HoneyCollector _collector;

        public BeeMiddleware(RequestDelegate next, HoneyCollector collector)
        {
            _next = next;
            _collector = collector;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request != null &&
                context.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path.HasValue &&
                context.Request.Path.Value.Equals("/api/bees", StringComparison.InvariantCultureIgnoreCase))
            {
                var cancellation = context?.RequestAborted ?? CancellationToken.None;

                context.Request.EnableBuffering();

                var honeySent =
                    await JsonSerializer.DeserializeAsync<HoneySentRequest>(
                        context.Request.Body,
                        JsonOptions,
                        cancellation);

                var name = honeySent.Name.ToLowerInvariant();

                _collector.RegisterHoneyAmount(name, honeySent.Amount);

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(honeySent.Name, cancellation);

                return;
            }

            await _next(context);
        }
    }
}