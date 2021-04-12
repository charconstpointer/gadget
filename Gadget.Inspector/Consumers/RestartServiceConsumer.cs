using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Commands.v1;
using Gadget.Messaging.Contracts.Responses;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Gadget.Inspector.Consumers
{
    public class RestartServiceConsumer : IConsumer<IRestartService>
    {
        private readonly ILogger<RestartServiceConsumer> _logger;
        private readonly ICollection<Service> _services;

        public RestartServiceConsumer(ILogger<RestartServiceConsumer> logger, ICollection<Service> services)
        {
            _logger = logger;
            _services = services;
        }


        public async Task Consume(ConsumeContext<IRestartService> context)
        {
            _logger.LogInformation($"Trying to start {context.Message.ServiceName}");
            var service = _services.FirstOrDefault(s => s.Name == context.Message.ServiceName);
            if (service is null)
            {
                throw new ApplicationException($"Service {context.Message.ServiceName} could not be found");
            }

            try
            {
                var timeout = TimeSpan.FromMilliseconds(500);
                await RestartRunningService(service, timeout);
                await context.RespondAsync<IActionResultResponse>(new
                {
                    Success = true
                });
            }
            catch
            {
                await context.RespondAsync<IActionResultResponse>(new
                {
                    Success = false
                });
                _logger.LogError($"Could not restart service {context.Message.Agent}{context.Message.ServiceName}");
            }
        }


        private static Task RestartRunningService(Service service, TimeSpan timeout)
        {
            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop(timeout);
            }

            service.Start(timeout);
            return Task.CompletedTask;
        }
    }
}