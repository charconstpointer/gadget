using System;
using System.Diagnostics;
using System.Threading.Channels;
using Gadget.Messaging;
using Gadget.Messaging.ServiceMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Gadget.Inspector.Extensions
{
    public static class InspectorExtensions
    {
        public static IServiceCollection AddInspector(this IServiceCollection services)
        {
            services.AddScoped(_ => Channel.CreateUnbounded<ServiceStatusChanged>());
            services.AddScoped(_ => Channel.CreateUnbounded<MachineHealthData>());
            services.AddTransient(_ => new Uri("https://localhost:5001/gadget"));
            services.AddTransient(_ => new PerformanceCounter());
            services.AddHostedService<Services.Inspector>();
            services.AddHostedService<MachineHealthWatcher>();
            return services;
        }
    }
}