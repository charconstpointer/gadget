using FluentValidation;
using Gadget.Notifications.Requests;
using Gadget.Notifications.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Gadget.Notifications.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationsService _notificationsService;

        public NotificationsController(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }


        [HttpGet("types")]
        public IActionResult GetNotifierTypes()
        {
            var types = _notificationsService.GetNotifierTypes();
            return Ok(types);
        }

        [HttpPost("{agentName}/{serviceName}/webhooks")]
        public async Task<IActionResult> CreateNotifier(string agentName, string serviceName,
            CreateWebhook createWebhook, CancellationToken cancellationToken)
        {
            await _notificationsService.RegisterNotifier(agentName, serviceName, createWebhook.Receiver,
                createWebhook.NotifierType, cancellationToken);
            return Created("", "");
        }

        [HttpPost("{agentName}/{serviceName}/deleteNotifier")]
        public async Task<IActionResult> DeleteNotifier(string agentName, string serviceName,
            DeleteWebhook deleteWebhook, CancellationToken cancellationToken)
        {
            await _notificationsService.DeleteNotifier(agentName, serviceName, deleteWebhook.Receiver,
                cancellationToken);
            return Ok();
        }

        [HttpGet("{agentName}/{serviceName}/webhooks")]
        public async Task<IActionResult> GetWebhooks(string agentName, string serviceName,
            CancellationToken cancellationToken)
        {
            var webhooks = await _notificationsService.GetWebhooks(agentName, serviceName, cancellationToken);
            return Ok(webhooks);
        }
    }
}