﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gadget.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gadget.Server.Services
{
    public class HealthCheckService : BackgroundService
    {
        private readonly IDictionary<string, Guid> _connectedClients;
        private readonly IHubContext<GadgetHub> _hubContext;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(ILogger<HealthCheckService> logger, IDictionary<string, Guid> connectedClients,
            IHubContext<GadgetHub> hubContext)
        {
            _logger = logger;
            _connectedClients = connectedClients;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var (key, value) in _connectedClients)
                {
                    await _hubContext.Clients.Client(key)
                        .SendAsync("GetServicesReport", stoppingToken);
                    _logger.LogInformation($"Fetching status for client [{key}/{value}]");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}