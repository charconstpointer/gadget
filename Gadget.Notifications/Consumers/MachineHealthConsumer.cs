using Gadget.Messaging.Contracts.Events.v1;
using Gadget.Messaging.SignalR.v1;
using Gadget.Notifications.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.Notifications.Consumers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MachineHealthConsumer : IConsumer<IMetricsData>
    {
        private readonly ILogger<MachineHealthConsumer> _logger;
        private readonly IHubContext<NotificationsHub> _hub;
        public MachineHealthConsumer(ILogger<MachineHealthConsumer> logger, IHubContext<NotificationsHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        public async Task Consume(ConsumeContext<IMetricsData> context)
        {
            await _hub.Clients.Group("dashboard").SendAsync("MachineHealthReceived", new MachineHealthData
            {
                Agent = context.Message.Agent,
                CpuPercentUsage = context.Message.CpuPercentUsage,
                MemoryFree = context.Message.MemoryFree,
                MemoryTotal = context.Message.MemoryTotal,
                DiscTotal = context.Message.DiscTotal,
                DiscOccupied = context.Message.DiscOccupied,
                ServicesCount = context.Message.ServicesCount,
                ServicesRunning = context.Message.ServicesRunning
            });
        }
    }
}
