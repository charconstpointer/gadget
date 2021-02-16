﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Gadget.Server.Agents.Dto;
using Gadget.Server.Dto;

namespace Gadget.Server.Services
{
    public interface IAgentsService
    {
        Task<IEnumerable<AgentDto>> GetAgents();
        Task<IEnumerable<ServiceDto>> GetServices(string agentName);
        Task StartService(string agentName, string serviceName);
        Task StopService(string agentName, string serviceName);
        Task RestartService(string agentName, string serviceName);
        //TODO we could modify GetEvents method and merge them together
        Task<IEnumerable<EventDto>> GetLatestEvents(int count);
        Task<IEnumerable<EventDto>> GetEvents(string agent, string service);
    }
}