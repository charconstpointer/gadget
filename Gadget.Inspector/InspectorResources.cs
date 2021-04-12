using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Gadget.Messaging.Contracts.Events.v1;
using Gadget.Messaging.SignalR.v1;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace Gadget.Inspector
{
    public class InspectorResources : BackgroundService
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly IPublishEndpoint _publishEndpoint;

        public InspectorResources(PerformanceCounter cpuCounter, IPublishEndpoint publishEndpoint)
        {
            _cpuCounter = cpuCounter;
            _publishEndpoint = publishEndpoint;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var metrics = CheckMachineHealth();
                var services = ServiceController.GetServices();
                metrics.ServicesCount = services.Length;
                metrics.ServicesRunning = services.Count(x => x.Status == ServiceControllerStatus.Running);
                await _publishEndpoint.Publish<IMetricsData>(metrics, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private MachineHealthData CheckMachineHealth()
        {
            var output = new MachineHealthData
            {
                Agent = Environment.MachineName,
                CpuPercentUsage = (int) _cpuCounter.NextValue(),
            };

            AddDrivesInfo(output);
            AddMemoryInfo(output);
            return output;
        }

        private static void AddDrivesInfo(MachineHealthData model)
        {
            var discs = DriveInfo.GetDrives();

            foreach (var d in discs)
            {
                model.DiscTotal += (int) (d.TotalSize / 1073741824f);
                model.DiscOccupied += (int) (d.AvailableFreeSpace / 1073741824f);
            }
        }

        private static void AddMemoryInfo(MachineHealthData model)
        {
            var qo = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(qo);
            var results = searcher.Get();
            var result = results.OfType<ManagementObject>().FirstOrDefault();
            if (result is null) return;
            if (int.TryParse(result["TotalVisibleMemorySize"].ToString(), out var total))
            {
                model.MemoryTotal = total / 1048576f;
            }

            if (int.TryParse(result["FreePhysicalMemory"].ToString(), out var free))
            {
                model.MemoryFree = free / 1048576f;
            }
        }
    }
}