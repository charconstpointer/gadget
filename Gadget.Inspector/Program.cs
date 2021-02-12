﻿using System;
using System.Diagnostics;
using System.Reflection;
using Gadget.Inspector.Consumers;
using MassTransit;
using MassTransit.ActivityTracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gadget.Inspector
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(config =>
                {
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((host, services) =>
                {
                    services.AddLogging(cfg => cfg.AddSeq());
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<StartServiceConsumer>();
                        x.AddConsumer<CheckAgentHealthConsumer>();
                        x.AddConsumer<StopServiceConsumer>();
                        x.AddConsumer<RestartServiceConsumer>();
                        x.AddConsumers(Assembly.GetExecutingAssembly());
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(host.Configuration.GetConnectionString("RabbitMq"),
                                configurator =>
                                {
                                    configurator.Username("guest");
                                    configurator.Password("guest");
                                });
                            cfg.ReceiveEndpoint(Environment.MachineName, e =>
                            {
                                e.ConfigureConsumer<StartServiceConsumer>(context);
                                e.ConfigureConsumer<StopServiceConsumer>(context);
                                e.ConfigureConsumer<RestartServiceConsumer>(context);
                                e.ConfigureConsumer<CheckAgentHealthConsumer>(context);
                            });
                            cfg.PropagateActivityTracingContext();
                        });
                    });
                    services.AddMassTransitHostedService();
                    services.AddHostedService<Inspector>();
                    services.AddHostedService<InspectorResources>();
                    services.AddLogging(options => options.AddConsole());
                    services.AddScoped(_ => new PerformanceCounter("Processor", "% Processor Time", "_Total"));
                    services.AddScoped<InspectorResources>();
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", false)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                });
        }
    }
}