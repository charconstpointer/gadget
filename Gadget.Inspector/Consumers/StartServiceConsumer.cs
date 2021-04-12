using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Commands.v1;
using Gadget.Messaging.Contracts.Events.v1;
using Gadget.Messaging.Contracts.Responses;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;

namespace Gadget.Inspector.Consumers
{
    public class StartServiceConsumer : IConsumer<IStartService>
    {
        private readonly ILogger<StartServiceConsumer> _logger;
        private readonly ICollection<Service> _services;

        public StartServiceConsumer(ILogger<StartServiceConsumer> logger, ICollection<Service> services)
        {
            _logger = logger;
            _services = services;
        }

        public async Task Consume(ConsumeContext<IStartService> context)
        {
            _logger.LogInformation($"Trying to start {context.Message.ServiceName}");
            var service = _services.FirstOrDefault(s => s.Name == context.Message.ServiceName);
            if (service is null)
            {
                throw new ApplicationException($"Service {context.Message.ServiceName} could not be found");
            }

            var timeout = TimeSpan.FromSeconds(5);
            try
            {
                await StartService(service, timeout);
                await context.RespondAsync<IActionResultResponse>(new
                {
                    Success = true
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.GetType().ToString());
                _logger.LogError(e.Message);
                await context.RespondAsync<IActionResultResponse>(new
                {
                    context.CorrelationId,
                    Agent = Environment.MachineName,
                    Service = service.Name,
                    Action = nameof(StartServiceConsumer),
                    Reason = e.Message,
                    Date = DateTime.UtcNow
                });
                await context.Publish<IActionFailed>(new
                {
                    context.CorrelationId,
                    Agent = Environment.MachineName,
                    Service = service.Name,
                    Action = nameof(StartServiceConsumer),
                    Reason = e.Message,
                    Date = DateTime.UtcNow
                });
            }
        }

        private Task StartService(Service service, TimeSpan timeout, int retries = 3)
        {
            Policy
                .Handle<Win32Exception>()
                .Or<InvalidOperationException>()
                .WaitAndRetry(retries, _ => timeout)
                .Execute(
                    () =>
                    {
                        _logger.LogInformation(
                            $"Trying to execute action {nameof(StartServiceConsumer)}/{nameof(StartService)}");
                        service.Start(timeout);
                    });
            return Task.CompletedTask;
        }
    }
}