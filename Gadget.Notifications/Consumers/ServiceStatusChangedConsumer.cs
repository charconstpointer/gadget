using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Events.v1;
using Gadget.Messaging.SignalR.v1;
using Gadget.Notifications.Domain.Enums;
using Gadget.Notifications.Domain.ValueObjects;
using Gadget.Notifications.Hubs;
using Gadget.Notifications.Persistence;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gadget.Notifications.Consumers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServiceStatusChangedConsumer : IConsumer<IServiceStatusChanged>
    {
        private readonly ILogger<ServiceStatusChangedConsumer> _logger;
        private readonly IHubContext<NotificationsHub> _hub;
        private readonly ChannelWriter<DiscordMessage> _discord;
        private readonly ChannelWriter<EmailMessage> _emails;
        private readonly NotificationsContext _notificationsContext;

        public ServiceStatusChangedConsumer(ILogger<ServiceStatusChangedConsumer> logger,
            IHubContext<NotificationsHub> hub, Channel<DiscordMessage> channel,
            NotificationsContext notificationsContext, Channel<EmailMessage> emails)
        {
            _logger = logger;
            _hub = hub;
            _notificationsContext = notificationsContext;
            _emails = emails;
            _discord = channel.Writer;
        }

        public async Task Consume(ConsumeContext<IServiceStatusChanged> context)
        {
            _logger.LogInformation(
                $"Service {context.Message.Name} has changed its status to {context.Message.Status}");
            try
            {
                await SendSignalRNotification(context);
                _logger.LogInformation("Trying to enqueue webhook notification");

                var notifiers = await _notificationsContext.Notifications
                    .Include(s => s.Notifiers)
                    .Where(n => n.Agent == context.Message.Agent && n.Service == context.Message.Name)
                    .AsNoTracking()
                    .SelectMany(s => s.Notifiers)
                    .ToListAsync(context.CancellationToken);

                foreach (var notifier in notifiers)
                {
                    await EnqueueMessage(notifier, context.Message.Status, context.Message.Agent, context.Message.Name);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return;
            }

            _logger.LogInformation("Invoked hub notification");
        }

        private async Task SendSignalRNotification(ConsumeContext<IServiceStatusChanged> context)
        {
            await _hub.Clients.Group("dashboard").SendAsync("ServiceStatusChanged", new ServiceDescriptor
            {
                Agent = context.Message.Agent,
                Name = context.Message.Name,
                Status = context.Message.Status
            }, context.CancellationToken);
        }

        private async Task EnqueueMessage(Receiver receiver, string status, string agent, string service)
        {
            switch (receiver.NotifierType)
            {
                case NotifierType.Discord:
                    var discordMessage = new DiscordMessage($"Agent : {agent} Service : {service} Status : {status}",
                        new Uri(receiver.Destination));
                    await _discord.WriteAsync(discordMessage);
                    _logger.LogInformation("Enqueued discord message");
                    break;
                case NotifierType.Email:
                    var emailMessage = new EmailMessage($"Agent : {agent} Service : {service} Status : {status}",
                        receiver.Destination);
                    await _emails.WriteAsync(emailMessage);
                    _logger.LogInformation("Enqueued email message");
                    break;
                case NotifierType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}