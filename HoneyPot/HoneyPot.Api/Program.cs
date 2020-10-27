using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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

                        builder.ConfigureKestrel(x =>
                        {
                            x.ListenAnyIP(80);
                        });

                        builder.Configure(app =>
                        {
                            app.UseRouting();

                            app.UseExceptionHandler(builder =>
                            {
                                builder.Run(
                                    async context =>
                                    {

                                        var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
                                        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                                        loggerFactory.CreateLogger("ExceptionHandler").LogError(exceptionHandler.Error, exceptionHandler.Error.Message, null);
                                    });
                            });

                            app.UseCors("AllowAllPolicy");

                            app.UseMiddleware<BeeMiddleware>();
                        });
                    })
                    .ConfigureServices((ctx, services) =>
                    {
                        services.Configure<HoneyPotServiceOptions>(ctx.Configuration.GetSection(nameof(HoneyPotServiceOptions)));

                        services.AddSingleton(new Queue<HoneySentNotification>());

                        services.AddMediatR(new[] { typeof(HoneySentNotification) });

                        services.AddTransient<INotificationHandler<HoneySentNotification>, HoneySentNotificationHandler>();

                        services.AddLogging(x => x.AddConsole());
                        services.AddCors();

                        services.AddHttpClient();

                        services.AddHostedService<HoneyPotService>();
                    })
                    .Build();

            return host.RunAsync();
        }
    }
}
