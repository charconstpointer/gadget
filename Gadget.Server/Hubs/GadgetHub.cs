﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gadget.Messaging.Commands;
using Gadget.Messaging.Events;
using Gadget.Server.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Gadget.Server.Hubs
{
    public class GadgetHub : Hub
    {
        private readonly ICollection<Agent> _agents;
        private readonly ILogger<GadgetHub> _logger;

        public GadgetHub(ICollection<Agent> agents, ILogger<GadgetHub> logger)
        {
            _agents = agents;
            _logger = logger;
        }


        public override Task OnDisconnectedAsync(Exception exception)
        {
            // var connectionId = Context.ConnectionId;
            // var agent = _agents.WithConnectionId(connectionId);
            // _agents.Remove(agent);
            // _logger.LogInformation($"Client {connectionId} has disconnected");
            return Task.CompletedTask;
        }

        public Task RegisterMachineReport(IRegisterNewAgent registerMachineReport)
        {
            _logger.LogInformation($"Received new machine report from agent {registerMachineReport.Agent}");
            var agent = _agents.FirstOrDefault(a => a.Name == registerMachineReport.Agent);
            if (agent is null)
            {
                _logger.LogError($"Could not find agent {registerMachineReport.Agent} for {nameof(IRegisterNewAgent)}");
                return Task.CompletedTask;
            }

            //TODO THIS IS BAD
            foreach (var service in registerMachineReport.Services)
            {
                var serviceUpdate = agent.Services.FirstOrDefault(s => s.Name == service.Name);
                serviceUpdate!.Status = service.Status;
                _logger.LogInformation($"Service name : {service.Name} status : {service.Status}");
            }

            return Task.CompletedTask;
        }

        //TODO Remove from group on disconnect
        public Task RegisterDashboard(IRegisterNewDashboard registerNewDashboard)
        {
            var connectionId = Context.ConnectionId;
            Groups.AddToGroupAsync(connectionId, "dashboard");
            return Task.CompletedTask;
        }

        public Task Register(IRegisterNewAgent registerNewAgent)
        {
            // var connectionId = Context.ConnectionId;
            // _logger.LogInformation($"Registering new agent {registerNewAgent.Agent} with CID : {connectionId}");
            // var agentId = registerNewAgent.Agent;
            // var agent = new Agent(agentId, connectionId);
            // agent.AddServices(registerNewAgent.Services.Select(s => new Service(s.Name, s.Status)));
            // _agents.Add(agent);
            return Task.CompletedTask;
        }

        public Task ServiceStatusChanged(ServiceStatusChanged serviceStatusChanged)
        {
            // var connectionId = Context.ConnectionId;
            // var agent = _agents.WithConnectionId(connectionId);
            // if (agent is null)
            // {
            //     _logger.LogError($"Cannot find agent for {connectionId}");
            //     return Task.CompletedTask;
            // }
            //
            // var serviceName = serviceStatusChanged.Name;
            // var serviceStatus = serviceStatusChanged.Status;
            // var service = agent.Services.Single(s =>
            //     string.Equals(s.Name, serviceName, StringComparison.CurrentCultureIgnoreCase));
            // service.Status = serviceStatus;
            // try
            // {
            //     Clients.Group("dashboard").SendAsync("ServiceStatusChanged", serviceStatusChanged);
            // }
            // catch (Exception exception)
            // {
            //     _logger.LogError(exception.Message);
            // }
            //
            return Task.CompletedTask;
        }
    }
}