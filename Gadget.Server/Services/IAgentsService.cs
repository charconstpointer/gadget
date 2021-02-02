﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Gadget.Server.Agents.Dto;

namespace Gadget.Server.Services
{
    public interface IAgentsService
    {
        Task<IEnumerable<AgentDto>> GetAgents();
        Task<IEnumerable<ServiceDto>> GetServices(string agentName);
        Task StartService(string agentName, string serviceName);
        Task StopService(string agentName, string serviceName);
        Task RestartService(string agentName, string serviceName);
        Task<IEnumerable<EventDto>> GetLatestEvents(int count);
    }
}