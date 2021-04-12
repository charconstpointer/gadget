using System.Collections.Generic;
using System.ServiceProcess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gadget.Inspector.Extensions
{
    public static class AgentBootstrapper
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            var requestedServices = configuration.GetSection("RequestedServices").Get<RequestedServices>();
            var s = new List<Service>();
            foreach (var requestedServicesService in requestedServices.Services)
            {
                var config = new ServiceConfiguration(ServiceAction.Restart, 3);
                var controller = new ServiceController(requestedServicesService.Name);
                var svc = new Service(controller, config);
                s.Add(svc);
            }

            services.AddSingleton<ICollection<Service>>(s);
            return services;
        }
    }
}