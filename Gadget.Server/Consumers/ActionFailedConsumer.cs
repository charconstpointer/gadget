using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Events.v1;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Gadget.Server.Consumers
{
    public class ActionFailedConsumer : IConsumer<IActionFailed>
    {
        private readonly ILogger<ActionFailedConsumer> _logger;

        public ActionFailedConsumer(ILogger<ActionFailedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IActionFailed> context)
        {
            _logger.LogError($"Could not execute action {context.Message.Action} {context.Message.Agent}/{context.Message.Service} on {context.Message.Date}, {context.Message.Reason}");
        }
    }
}