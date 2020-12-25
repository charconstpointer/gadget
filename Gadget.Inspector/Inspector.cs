using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Gadget.Messaging.Commands;
using Gadget.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gadget.Inspector
{
    public class Inspector : BackgroundService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<Inspector> _logger;

        private readonly IDictionary<string, ServiceControllerStatus> _services =
            new Dictionary<string, ServiceControllerStatus>();

        public Inspector(IPublishEndpoint publishEndpoint, ILogger<Inspector> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _publishEndpoint.Publish<IRegisterNewAgent>(new
            {
                Agent = Environment.MachineName,
                // Services = ServiceController.GetServices().Select(s => new Service
                // {
                //     Name = s.ServiceName,
                //     Status = s.Status.ToString()
                // })
                Services = Enumerable.Empty<object>()
            }, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var serviceController in ServiceController.GetServices())
                {
                    serviceController.Refresh();
                    var current = serviceController.Status;
                    if (!_services.TryGetValue(serviceController.ServiceName, out var previous))
                    {
                        _services[serviceController.ServiceName] = current;
                        continue;
                    }

                    if (current == previous)
                    {
                        continue;
                    }
                    _services[serviceController.ServiceName] = current;
                    await _publishEndpoint.Publish<IServiceStatusChanged>(new
                    {
                        Agent = Environment.MachineName,
                        ServicName = serviceController.ServiceName,
                        Status = current.ToString()
                    }, stoppingToken);

                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}