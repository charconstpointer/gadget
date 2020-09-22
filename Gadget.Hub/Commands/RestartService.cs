﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Gadget.Hub.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Gadget.Hub.Commands
{
    public class RestartService : IRequest
    {
        public string ConnectionId { get; set; }
        public string ServiceName { get; set; }
    }
    
    public class RestartServiceHandler : IRequestHandler<RestartService>
    {
        private readonly IHubContext<GadgetHub> _hubContext;

        public RestartServiceHandler(IHubContext<GadgetHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task<Unit> Handle(RestartService request, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.Client(request.ConnectionId).SendAsync("RestartService");
        }
    }
}