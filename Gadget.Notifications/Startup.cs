using FluentValidation.AspNetCore;
using Gadget.Notifications.Consumers;
using Gadget.Notifications.Extensions;
using Gadget.Notifications.Hubs;
using Gadget.Notifications.Persistence;
using Gadget.Notifications.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gadget.Notifications
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(cfg => cfg.AddSeq());
            services.AddTransient<INotificationsService, NotificationsService>();
            services.AddEmailNotifications(Configuration);
            services.AddWebhooksNotifications();
            services.AddValidations();
            services.AddDbContext<NotificationsContext>(builder => builder.UseSqlite("Data Source=notifications.db"));
            services.AddSignalR();
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ServiceStatusChangedConsumer>()
                    .Endpoint(cfg => cfg.Name = $"Notifications-{nameof(ServiceStatusChangedConsumer)}");
                x.AddConsumer<MachineHealthConsumer>()
                    .Endpoint(cfg => cfg.Name = $"Notifications-{nameof(MachineHealthConsumer)}");
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration.GetConnectionString("RabbitMq"),
                        configurator =>
                        {
                            configurator.Username("guest");
                            configurator.Password("guest");
                        });

                    cfg.ConfigureEndpoints(context);
                });
            });
            services.AddMassTransitHostedService();
            services.AddControllers()
                .AddFluentValidation();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    corsBuilder =>
                    {
                        corsBuilder
                            .WithOrigins("localhost:3000")
                            .WithOrigins("http://localhost:3000")
                            .WithOrigins("localhost:5000")
                            .WithOrigins("http://localhost:5000")
                            .WithOrigins("https://localhost:5000")
                            .WithOrigins("http://localhost:5001")
                            .WithOrigins("https://localhost:5001")
                            .WithOrigins("https://localhost:5005")
                            .WithOrigins("https://localhost:5006")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<NotificationsContext>())
                {
                    logger.LogCritical("ensurecreated");
                    context.Database.EnsureCreated();
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => await context.Response.WriteAsync(":))"));
                endpoints.MapHub<NotificationsHub>("/gadget");
                endpoints.MapControllers();
            });
        }
    }
}