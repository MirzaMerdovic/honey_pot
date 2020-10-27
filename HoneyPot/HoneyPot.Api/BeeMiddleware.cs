using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class BeeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _factory;
        private readonly IMediator _mediator;

        private readonly Stopwatch _watch = new Stopwatch();

        public BeeMiddleware(RequestDelegate next, IHttpClientFactory factory, IMediator mediator)
        {
            _next = next;
            _factory = factory;
            _mediator = mediator;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request != null &&
                context.Request.Path.HasValue &&
                context.Request.Path.Value.Equals("/api/bees", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Query.ContainsKey("id"))
            {
                var id = context.Request.Query["id"];
                var client = _factory.CreateClient();

                _watch.Restart();
                _ = await client.GetAsync(new Uri($"https://restcountries.eu/rest/v2/name/{id}"));
                var elapsed = _watch.ElapsedMilliseconds;

                await _mediator.Publish(new HoneySentNotification { Name = id, TimeTook = elapsed });

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");

                return;
            }

            await _next(context);
        }
    }
}