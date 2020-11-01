using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class Program
    {
        internal static Task Main(string[] args)
        {
            var host =
                new HostBuilder()
                    .ConfigureAppConfiguration(builder =>
                    {
                        builder
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, true)
                            .AddEnvironmentVariables()
                            .Build();
                    })
                    .ConfigureWebHostDefaults(builder =>
                    {
                        builder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

                        builder.Configure(app =>
                        {
                            app.UseRouting();

                            app.UseExceptionHandler(builder =>
                            {
                                builder.Run(
                                    context =>
                                    {
                                        var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
                                        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                                        loggerFactory.CreateLogger("ExceptionHandler").LogError(exceptionHandler.Error, exceptionHandler.Error.Message, null);

                                        return Task.CompletedTask;
                                    });
                            });

                            app.UseCors("AllowAllPolicy");

                            app.UseMiddleware<BeeMiddleware>(Array.Empty<object>());
                        });
                    })
                    .ConfigureServices((ctx, services) =>
                    {
                        services.Configure<HoneyPotServiceOptions>(ctx.Configuration.GetSection(nameof(HoneyPotServiceOptions)));
                        services.Configure<RedisCacheOptions>(ctx.Configuration.GetSection(nameof(RedisCacheOptions)));

                        services.AddSingleton(new HoneyCollector());

                        services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Warning));
                        services.AddCors();

                        services.AddHttpClient();
                        services.AddStackExchangeRedisCache(x =>
                        {
                            x.Configuration = ctx.Configuration["ConnectionStrings:Redis"];
                            x.InstanceName = "local";
                        });

                        services.AddHostedService<HoneyPotService>();
                    })
                    .Build();

            return host.RunAsync();
        }
    }
}
