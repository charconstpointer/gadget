using Gadget.Messaging.Contracts.Commands;
using Gadget.Server.Authorization;
using Gadget.Server.Authorization.Helpers;
using Gadget.Server.Authorization.Providers;
using Gadget.Server.Authorization.Services;
using Gadget.Server.Authorization.Services.Interfaces;
using Gadget.Server.Consumers;
using Gadget.Server.HealthCheck;
using Gadget.Server.Persistence;
using Gadget.Server.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gadget.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TokenManager>();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                var secret = Configuration.GetValue<string>("SecurityKey");
                var key = Encoding.ASCII.GetBytes(secret);
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            services.AddLogging(cfg => cfg.AddSeq());
            services.AddDbContext<GadgetContext>(builder => builder.UseSqlite("Data Source=gadget.db"));
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ServiceStatusChangedConsumer>();
                x.AddConsumer<RegisterNewAgentConsumer>();
                x.AddConsumer<ActionFailedConsumer>();
                x.AddRequestClient<CheckAgentHealth>();
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
                            .WithOrigins("https://localhost:5005")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });
            services.AddControllers();
            services.AddTransient<IAgentsService, AgentsService>();
            services.AddHostedService<AgentHealthCheck>();
            services.AddTransient<ILoginProvider, PrMockProvider>();
            services.AddTransient<IUsersService, UsersService>();
            services.AddTransient<AuthorizationHelper>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<GadgetContext>())
                {
                    logger.LogCritical("ensurecreated");
                    context?.Database.EnsureCreated();
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");
            app.UseFileServer();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}