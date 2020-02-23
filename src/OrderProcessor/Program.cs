using System;
using System.Configuration;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MongoDbIntegration;
using MassTransit.Saga;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderProcessor.Services;
using OrderProcessor.State;


namespace OrderProcessor
{
    class Program
    {
        public static async Task Main(string[] args)
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
                .Configure<RabbitMqOptions>(options => hostContext.Configuration.GetSection("RabbitMQ").Bind(options))
                .Configure<MongoDbOptions>(options => hostContext.Configuration.GetSection("MongoDb").Bind(options))
                .AddMassTransit(x =>
                {
                    x.AddSagaStateMachine<OrderProcessorStateMachine, ProcessingOrderState>()
                        .MongoDbRepository(r =>
                        {
                            var mongoDbSettings = hostContext.Configuration.GetSection("MongoDb").Get<MongoDbOptions>();
                            r.Connection = mongoDbSettings.Host;
                            r.DatabaseName = mongoDbSettings.Database;
                            r.CollectionName = mongoDbSettings.Collection;
                        });
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
                            e.StateMachineSaga(provider.GetService<OrderProcessorStateMachine>(),
                                provider.GetService<ISagaRepository<ProcessingOrderState>>());
                        });
                    }));
                })
                .AddHostedService<BusService>();
        }
    }
}
