using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Events;
using Gadget.Messaging.SignalR;
using Gadget.Server.Domain.Entities;
using Gadget.Server.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gadget.Server.Agents.Consumers
{
    public class ServiceStatusChangedConsumer : IConsumer<IServiceStatusChanged>
    {
        private readonly ILogger<ServiceStatusChangedConsumer> _logger;
        private readonly GadgetContext _context;
        private readonly Channel<Notification> _notifications;

        public ServiceStatusChangedConsumer(ILogger<ServiceStatusChangedConsumer> logger, GadgetContext context,
            Channel<Notification> notifications)
        {
            _logger = logger;
            _context = context;
            _notifications = notifications;
        }

        public async Task Consume(ConsumeContext<IServiceStatusChanged> context)
        {
            var agentName = context.Message.Agent;
            var service = context.Message.Name;
            var newStatus = context.Message.Status;

            var agent = await _context.Agents
                .Include(a => a.Services)
                .ThenInclude(s => s.Events.Take(1))
                .FirstOrDefaultAsync(a => a.Name == agentName);
            if (agent == null)
            {
                throw new ApplicationException($"Agent {agentName} is not registered");
            }

            agent.ChangeServiceStatus(service, newStatus);
            _context.Agents.Update(agent);
            await _context.SaveChangesAsync();
            await _notifications.Writer.WriteAsync(new Notification(agent.Name, service, newStatus));
            _logger.LogInformation(
                $"Agent {context.Message.Agent} Svc {context.Message.Name} Status {context.Message.Status}");
        }
    }
}