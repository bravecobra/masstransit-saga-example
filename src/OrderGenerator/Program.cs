using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SagasDemo.OrderGenerator.Consumers;
using ServiceModel.Events;

namespace SagasDemo.OrderGenerator
{
    class Program
    {
        private static bool continueRunning = true;
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

                    x.AddConsumer<OrderCancelledConsumer>();
                    x.AddConsumer<OrderProcessedConsumer>();
                    x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        var settings = provider.GetRequiredService<IOptions<RabbitMqOptions>>();
                        var rabbitmqSettings = settings.Value;
                        cfg.Host(rabbitmqSettings.Host, h =>
                        {
                            h.Username(rabbitmqSettings.User);
                            h.Password(rabbitmqSettings.Password);
                        });
                        cfg.ReceiveEndpoint(rabbitmqSettings.InputQueue, e =>
                        {
                            e.ConfigureConsumer<OrderCancelledConsumer>(provider);
                            e.ConfigureConsumer<OrderProcessedConsumer>(provider);
                        });
                    }));
                })
                .AddHostedService<BusService>()
                .AddHostedService<ConsoleApplication>();
        }

        // private static Task GenerateOrders(IServiceProvider provider, CancellationToken cancellationToken)
        // {
        //     Console.WriteLine("Enter to send..");
        //     Console.ReadLine();
        //
        //     var bus = provider.GetRequiredService<IBusControl>();
        //
        //     while (!cancellationToken.IsCancellationRequested)
        //     {             
        //         var order = Services.OrderGenerator.Currrent.Generate();
        //         bus.Publish<IOrderSubmitted>(new { CorrelationId = Guid.NewGuid(), Order = order }, cancellationToken);
        //         Console.WriteLine(order.ToString());
        //         Console.ReadLine();
        //     }
        //
        //     return Task.CompletedTask;
        // }
    }
}
