using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Commands.v1;
using Gadget.Messaging.Contracts.Responses;
using Gadget.Server.Dto.V1;
using Gadget.Server.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gadget.Server.Services
{
    public class AgentsService : IAgentsService
    {
        private readonly GadgetContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IBus _bus;
        private readonly ILogger<AgentsService> _logger;

        public AgentsService(GadgetContext context, IPublishEndpoint publishEndpoint, ILogger<AgentsService> logger,
            IBus bus)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _bus = bus;
        }

        public async Task<IEnumerable<AgentDto>> GetAgents()
        {
            var agents = await _context.Agents.ToListAsync();
            return await Task.FromResult(agents.Select(a => new AgentDto(a.Name, a.Address)));
        }

        public async Task<IEnumerable<EventDto>> GetLatestEvents(int count)
        {
            var events = await _context.ServiceEvents
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.Service)
                .ThenInclude(x => x.Agent)
                .Take(count)
                .ToListAsync();

            return await Task.FromResult(events.Select(e =>
                new EventDto(e.Service.Agent.Name, e.Service.Name, e.CreatedAt, e.Status)));
        }

        public async Task<IEnumerable<EventDto>> GetEvents(string agent, string serviceName, DateTime from, DateTime to,
            int count = int.MaxValue, int skip = 0)
        {
            if (from == DateTime.MinValue)
            {
                from = DateTime.UtcNow.AddYears(-1);
            }

            if (to == DateTime.MinValue)
            {
                to = DateTime.UtcNow;
            }

            var events = await _context.ServiceEvents
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.Service)
                .ThenInclude(x => x.Agent)
                .Where(x =>
                    x.Service.Name == serviceName &&
                    x.Service.Agent.Name == agent &&
                    x.CreatedAt > from &&
                    x.CreatedAt < to)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            return await Task.FromResult(events.Select(e =>
                new EventDto(e.Service.Agent.Name, e.Service.Name, e.CreatedAt, e.Status)));
        }

        public async Task<IEnumerable<EventDto>> GetEvents(string agent, string service)
        {
            var ag = await _context.Agents
                .Include(a => a.Services)
                .ThenInclude(s => s.Events)
                .FirstOrDefaultAsync(a => a.Name == agent);

            var svc = ag.Services.FirstOrDefault(s => s.Name == service);
            return svc?.Events.Select(e => new EventDto(e.Service.Agent.Name, e.Service.Name, e.CreatedAt, e.Status));
        }

        public async Task<IEnumerable<ServiceDto>> GetServices(string agentName)
        {
            var machine = _context.Agents
                .Include(a => a.Services)
                .ThenInclude(s => s.Events.Take(20))
                .FirstOrDefault(x => x.Name == agentName);
            var services = machine?.Services;
            var dto = services?.Select(s => new ServiceDto(s.Name, s.Status, s.LogOnAs, s.Description));
            return await Task.FromResult(dto);
        }

        public async Task RestartService(string agentName, string serviceName)
        {
            var service = $"{agentName}/{serviceName}";
            try
            {
                var client = _bus.CreateRequestClient<IRestartService>(new Uri($"queue:{agentName}"));
                var response = await client.GetResponse<IActionResultResponse>(new
                {
                    Agent = agentName,
                    ServiceName = serviceName
                });
                if (!response.Message.Success)
                {
                    _logger.LogError($"could not restart service {service}");
                    return;
                }

                _logger.LogInformation($"Successfully restarted service {service}");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
            }
        }

        public async Task StartService(string agentName, string serviceName)
        {
            var service = $"{agentName}/{serviceName}";
            try
            {
                var client = _bus.CreateRequestClient<IStartService>(new Uri($"queue:{agentName}"));
                var response = await client.GetResponse<IActionResultResponse>(new
                {
                    Agent = agentName,
                    ServiceName = serviceName
                });
                if (!response.Message.Success)
                {
                    _logger.LogError($"could not start service {service}");
                    return;
                }

                _logger.LogInformation($"Successfully started service {service}");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
            }
        }

        public async Task StopService(string agentName, string serviceName)
        {
            try
            {
                await _publishEndpoint.Publish<IStopService>(new
                    {
                        ServiceName = serviceName,
                        Agent = agentName
                    },
                    context => { context.SetRoutingKey(serviceName); });
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
            }
        }
    }
}