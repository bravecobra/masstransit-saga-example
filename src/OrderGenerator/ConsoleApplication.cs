using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceModel.Events;

namespace SagasDemo.OrderGenerator
{
    public class ConsoleApplication : IHostedService
    {
        private readonly ILogger<ConsoleApplication> logger;
        private readonly IBusControl _busControl;

        public ConsoleApplication(ILogger<ConsoleApplication> logger, IBusControl busControl)
        {
            this.logger = logger;
            _busControl = busControl;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting application");

            Task.Run(() => {
                bool quit = false;
                while (!quit) {
                    Console.WriteLine("Enter to send..");

                    var order = Services.OrderGenerator.Currrent.Generate();
                    _busControl.Publish<IOrderSubmitted>(new { CorrelationId = Guid.NewGuid(), Order = order }, cancellationToken);
                    Console.WriteLine(order.ToString());

                    string action = Console.ReadLine();
                    quit = action == "quit";
                }

            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Ending the application");

            return Task.CompletedTask;
        }
    }
}
