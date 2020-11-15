using System;
using System.Threading.Channels;
using Gadget.Inspector.Transport;
using Gadget.Messaging.Events;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Gadget.Inspector.Extensions
{
    public static class InspectorExtensions
    {
        public static IServiceCollection AddInspector(this IServiceCollection services)
        {
            services.AddScoped(_ =>
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithAutomaticReconnect()
                    .WithUrl("https://localhost:5001/gadget")
                    .Build();
                hubConnection.StartAsync().GetAwaiter().GetResult();
                return hubConnection;
            });
            services.AddScoped<IControlPlane, ControlPlane>();
            services.AddScoped(_ => Channel.CreateUnbounded<ServiceStatusChanged>());
            services.AddTransient(_ => new Uri("https://localhost:5001/gadget"));
            services.AddHostedService<Services.Inspector>();
            return services;
        }
    }
}