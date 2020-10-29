using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class BeeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Queue<HoneySentRequest> _notifications;

        public BeeMiddleware(RequestDelegate next, Queue<HoneySentRequest> notifications)
        {
            _next = next;
            _notifications = notifications;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request != null &&
                context.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path.HasValue &&
                context.Request.Path.Value.Equals("/api/bees", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);
                string bodyStr = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                var honeySent = JsonSerializer.Deserialize<HoneySentRequest>(bodyStr);

                _notifications.Enqueue(honeySent);

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");

                return;
            }

            await _next(context);
        }
    }
}