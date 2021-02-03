﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Gadget.Notifications.Domain.Entities;
using Gadget.Notifications.Domain.ValueObjects;
using Gadget.Notifications.Persistence;
using Gadget.Notifications.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gadget.Notifications.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationsContext _context;

        public NotificationsController(NotificationsContext context)
        {
            _context = context;
        }

        [HttpPost("{agentName}/{serviceName}")]
        public async Task<IActionResult> CreateNotification(string agentName, string serviceName)
        {
            var notification = new Notif(agentName, serviceName);
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return Created("", "");
        }

        [HttpPost("{agentName}/{serviceName}/webhooks")]
        public async Task<IActionResult> CreateWebhook(string agentName, string serviceName,
            CreateWebhook createWebhook)
        {
            var notification = new Notif(agentName, serviceName);
            var webhook = new Webhook(new Uri(createWebhook.Uri));
            notification.AddWebhook(webhook);
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return Created("", "");
        }

        [HttpGet("{agentName}/webhooks")]
        public async Task<IActionResult> GetWebhooks(string agentName)
        {
            var webhooks = await _context.Notifications
                .Where(s => s.Agent == agentName)
                .ToListAsync();
            return Ok(webhooks);
        }
    }
}