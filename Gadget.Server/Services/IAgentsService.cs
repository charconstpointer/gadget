﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gadget.Server.Dto.V1;

namespace Gadget.Server.Services
{
    public interface IAgentsService
    {
        Task<IEnumerable<AgentDto>> GetAgents();
        Task<IEnumerable<ServiceDto>> GetServices(string agentName);
        Task<Guid> StartService(string agentName, string serviceName);
        Task<Guid> StopService(string agentName, string serviceName);

        Task<Guid> RestartService(string agent, string serviceName);

        //TODO we could modify GetEvents method and merge them together
        Task<IEnumerable<EventDto>> GetLatestEvents(int count);

        Task<IEnumerable<EventDto>> GetEvents(string agent, string serviceName, DateTime from, DateTime to,
            int count = int.MaxValue, int skip = 0);
    }
}