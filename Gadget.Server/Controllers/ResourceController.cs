﻿using System.Linq;
using System.Threading.Tasks;
using Gadget.Server.Dto.V1.Requests;
using Gadget.Server.Persistence;
using Gadget.Server.Services;
using Gadget.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gadget.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly ISelectorService _selectorService;
        private readonly IAgentsService _agentsService;
        private readonly GadgetContext _gadgetContext;
        private readonly ILogger<ResourceController> _logger;

        public ResourceController(ISelectorService selectorService, IAgentsService agentsService,
            GadgetContext gadgetContext, ILogger<ResourceController> logger)
        {
            _selectorService = selectorService;
            _agentsService = agentsService;
            _gadgetContext = gadgetContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetResource(string selector)
        {
            var resources = await _selectorService.Match(selector);
            return resources is not null ? Ok(resources) : NotFound();
        }

        [HttpPost("config/apply")]
        public async Task<IActionResult> ApplyConfig(ApplyConfigRequest request)
        {
            var service = request.Rules.First().Selector;

            var svc = await _gadgetContext.Services
                .FirstOrDefaultAsync(s => s.Name == service.ToLower().Trim());
            _logger.LogInformation((svc is null).ToString());
            if (svc is null)
            {
                return BadRequest();
            }

            return Ok(service);
        }
    }
}