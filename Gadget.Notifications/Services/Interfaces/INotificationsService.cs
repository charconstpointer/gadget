using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gadget.Notifications.Domain.Entities;
using Gadget.Notifications.Domain.Enums;
using Gadget.Notifications.Domain.ValueObjects;
using Gadget.Notifications.Dto;
using Gadget.Notifications.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gadget.Notifications.Services.Interfaces
{
    public interface INotificationsService
    {
        Task RegisterNotifier(string agentName, string serviceName, string destination, NotifierType notifierType,
            CancellationToken cancellationToken);

        Task<NotificationDto> GetWebhooks(string agentName, string serviceName,
            CancellationToken cancellationToken);

        IEnumerable<object> GetNotifierTypes();

        Task DeleteNotifier(string agentName, string serviceName, string destination,
            CancellationToken cancellationToken);

        Task<IEnumerable<AgentDto>> GetAgentsAsync();
    }

    public class NotificationsService : INotificationsService
    {
        private readonly ILogger<NotificationsService> _logger;
        private readonly NotificationsContext _notificationsContext;

        public NotificationsService(ILogger<NotificationsService> logger, NotificationsContext notificationsContext)
        {
            _logger = logger;
            _notificationsContext = notificationsContext;
        }

        public async Task DeleteNotifier(string agentName, string serviceName, string destination,
            CancellationToken cancellationToken)
        {
            var notification = await _notificationsContext.Notifications
                .Include(x => x.Notifiers)
                .FirstOrDefaultAsync(x =>
                    x.Agent == agentName &&
                    x.Service == serviceName);
            if (notification is null)
            {
                _logger.LogInformation(
                    $"Trying to delete nonexistent Notification. Agent:{agentName}, Service:{serviceName}");
                return;
            }

            var toDelete = notification.Notifiers.FirstOrDefault(x => x.Destination == destination);
            if (toDelete is null)
            {
                _logger.LogInformation(
                    $"Trying to delete nonexistent Notifier. Agent:{agentName}, Service:{serviceName}");
                return;
            }

            notification.DeleteNotifier(toDelete);
            await _notificationsContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<AgentDto>> GetAgentsAsync()
        {
            return await _notificationsContext.Notifications
                .Select(n => n.Agent)
                .Distinct()
                .Select(n=>new AgentDto(n))
                .ToListAsync();
        }

        public async Task RegisterNotifier(string agentName, string serviceName, string destination,
            NotifierType notifierType,
            CancellationToken cancellationToken)
        {
            var notification = await _notificationsContext.Notifications
                .Include(x => x.Notifiers)
                .FirstOrDefaultAsync(x =>
                    x.Agent == agentName &&
                    x.Service == serviceName);

            var newNotifier = new Receiver(agentName, serviceName, destination, notifierType);

            if (notification is null)
            {
                notification = new Notification(agentName, serviceName);
                notification.AddNotifier(newNotifier);
                await _notificationsContext.Notifications.AddAsync(notification, cancellationToken);
                await _notificationsContext.SaveChangesAsync(cancellationToken);
                return;
            }

            var notifier = notification.Notifiers.FirstOrDefault(x =>
                x.Destination == destination &&
                x.NotifierType == notifierType);

            if (notifier is null)
            {
                notification.AddNotifier(newNotifier);
                await _notificationsContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<NotificationDto> GetWebhooks(string agentName, string serviceName,
            CancellationToken cancellationToken)
        {
            var notification = await _notificationsContext.Notifications
                .Where(n => n.Agent == agentName && n.Service == serviceName)
                .Include(n => n.Notifiers)
                .FirstOrDefaultAsync(cancellationToken);
            return new NotificationDto
            {
                Agent = notification.Agent,
                Id = notification.Id,
                Notifiers = notification.Notifiers.Select(nn => new NotifierDto
                {
                    Receiver = nn.Destination,
                    CreatedAt = nn.CreatedAt,
                    Type = nn.NotifierType.ToString(),
                }),
                Service = notification.Service
            };
        }

        public IEnumerable<object> GetNotifierTypes()
        {
            var names = Enum.GetNames(typeof(NotifierType)).ToList();
            return names.Select((x, i) => new {key = i, name = x});
        }
    }
}