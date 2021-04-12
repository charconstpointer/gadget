using Gadget.Messaging.Contracts.Commands;
using MassTransit;
using System.Threading.Tasks;

namespace Gadget.Inspector.Consumers
{
    public class CheckAgentHealthConsumer : IConsumer<CheckAgentHealth>
    {
        public async Task Consume(ConsumeContext<CheckAgentHealth> context)
        {
            await context.RespondAsync<CheckAgentHealth>(new {IsAlive = true});
        }
    }
}