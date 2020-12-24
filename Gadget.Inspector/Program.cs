﻿using System;
using System.Reflection;
using Gadget.Messaging.Commands;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

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
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumers(Assembly.GetExecutingAssembly());
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            var id = Environment.MachineName;
                            cfg.ReceiveEndpoint(id, e =>
                            {
                                e.Bind<IStopService>(c =>
                                {
                                    c.RoutingKey = id;
                                    c.ExchangeType = ExchangeType.Direct;
                                });
                                e.Bind<IStartService>(c =>
                                {
                                    c.RoutingKey = id;
                                    c.ExchangeType = ExchangeType.Direct;
                                });
                            });
                        });
                    });
                    services.AddMassTransitHostedService();
                    services.AddLogging(options => options.AddConsole());
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