using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Gadget.Messaging;
using Gadget.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gadget.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    corsBuilder =>
                    {
                        corsBuilder
                            .WithOrigins("localhost:3000")
                            .WithOrigins("http://localhost:3000")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
                ;
            });

            services.AddSignalR();
            services.AddControllers();
            services.AddSingleton<IDictionary<Guid, ICollection<Service>>>(_ =>
                new ConcurrentDictionary<Guid, ICollection<Service>>());
            services.AddSingleton<IDictionary<string, Guid>>(_ => new Dictionary<string, Guid>());
            // services.AddHostedService<HealthCheckService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseCors("AllowAll");
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GadgetHub>("/gadget");
                endpoints.MapControllers();
            });
        }
    }
}