using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cashier.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cashier
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddConsole().AddDebug())
                .ConfigureServices((hostContext, services) => { ConfigureServices(services, hostContext); })
                .RunConsoleAsync();
        }

        private static void ConfigureServices(IServiceCollection services, HostBuilderContext hostContext)
        {
            services
                .AddOptions()
                .Configure<RabbitMqOptions>(options =>
                    hostContext.Configuration.GetSection("RabbitMQ").Bind(options))
                .AddMassTransit(x =>
                {
                    x.AddConsumer<ProcessPaymentConsumer>();

                    x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        var settings = provider.GetRequiredService<IOptions<RabbitMqOptions>>();
                        var rabbitmqSettings = settings.Value;
                        cfg.Host(rabbitmqSettings.Host, h =>
                        {
                            h.Username(rabbitmqSettings.User);
                            h.Password(rabbitmqSettings.Password);
                        });
                        cfg.ReceiveEndpoint(rabbitmqSettings.InputQueue, e => { e.ConfigureConsumer<ProcessPaymentConsumer>(provider); });
                    }));
                })
                .AddHostedService<BusService>();
        }
    }
}
