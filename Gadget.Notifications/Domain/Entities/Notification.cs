﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Gadget.Notifications.Domain.ValueObjects;

namespace Gadget.Notifications.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; }
        public string Agent { get; }
        public string Service { get; }
        private readonly ISet<Webhook> _webhooks = new HashSet<Webhook>();
        public IEnumerable<Webhook> Webhooks => _webhooks.ToImmutableList();

        public Notification(string agent, string service)
        {
            Id = Guid.NewGuid();
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void AddWebhook(Webhook webhook)
        {
            _webhooks.Add(webhook);
        }
    }
}