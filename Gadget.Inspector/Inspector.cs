using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Commands.v1;
using Gadget.Messaging.Contracts.Events.v1;
using Gadget.Messaging.SignalR.v1;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gadget.Inspector
{
    public class Inspector : BackgroundService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<Inspector> _logger;
        private readonly int _loopInterval;

        private readonly IDictionary<string, ServiceControllerStatus> _services =
            new Dictionary<string, ServiceControllerStatus>();

        private readonly ICollection<Service> _svc;

        public Inspector(IPublishEndpoint publishEndpoint, ILogger<Inspector> logger, IConfiguration configuration,
            ICollection<Service> svc)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _svc = svc;
            _loopInterval = int.TryParse(configuration["MainLoopInterval"], out var interval) ? interval : 1;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Starting watcher {DateTime.UtcNow}");
            await RegisterAgent(stoppingToken);

            // while (!stoppingToken.IsCancellationRequested)
            // {
            //     foreach (var serviceController in ServiceController.GetServices())
            //     {
            //         await WatchForServiceChanges(serviceController, stoppingToken);
            //     }
            //
            //     await Task.Delay(TimeSpan.FromSeconds(_loopInterval), stoppingToken);
            // }
        }

        private async Task WatchForServiceChanges(ServiceController serviceController, CancellationToken stoppingToken)
        {
            serviceController.Refresh();
            var current = serviceController.Status;
            if (!_services.TryGetValue(serviceController.ServiceName, out var previous))
            {
                _services[serviceController.ServiceName] = current;
                return;
            }

            if (current == previous)
            {
                return;
            }

            _services[serviceController.ServiceName] = current;
            await _publishEndpoint.Publish<IServiceStatusChanged>(new
            {
                Agent = Environment.MachineName,
                Name = serviceController.ServiceName,
                Status = current.ToString()
            }, stoppingToken);
        }

        private async Task RegisterAgent(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Registering new agent {Environment.MachineName}");
            foreach (var service in _svc)
            {
                _ = service.Watch(stoppingToken);
            }

            await _publishEndpoint.Publish<IRegisterNewAgent>(new
            {
                Agent = Environment.MachineName,
                Address = GetAddress(),
                Services = _svc.Select(s => new ServiceDescriptor
                {
                    Agent = Environment.MachineName,
                    Name = s.Name,
                    Status = s.Status.ToString(),
                })
            }, stoppingToken);
        }

        private static string GetAddress()
        {
            var addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            return addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString();
        }

        private static string GetServiceUser(string serviceName)
        {
            var output = "";
            var command = $"Win32_Service.Name='{serviceName}'";
            var wmiService = new ManagementObject(command);
            wmiService.Get();
            try
            {
                output = wmiService["startname"].ToString();
            }
            catch (Exception)
            {
                // ignored
            }

            return output;
        }

        private static string GetServiceDescription(string serviceName)
        {
            var output = "";
            var command = $"Win32_Service.Name='{serviceName}'";
            var wmiService = new ManagementObject(command);
            try
            {
                output = wmiService["Description"].ToString();
            }
            catch (Exception)
            {
                // ignored
            }

            return output;
        }
    }
}