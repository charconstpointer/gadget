using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using TimeoutException = System.ServiceProcess.TimeoutException;

namespace Gadget.Inspector
{
    public enum ServiceAction
    {
        Ignore,
        Restart
    }

    public record ServiceConfiguration(ServiceAction OnFailure, int Retries);

    public class Service
    {
        private readonly ServiceController _controller;
        private readonly ServiceConfiguration _configuration;
        private bool _healthy = true;
        private ServiceControllerStatus _status;
        public string Name { get; }
        public ServiceControllerStatus Status { get; private set; }

        public Service(ServiceController controller, ServiceConfiguration configuration)
        {
            _controller = controller;
            _configuration = configuration;
            Name = controller.ServiceName;
            Status = controller.Status;
        }

        public void Start(TimeSpan timeout)
        {
            _controller.Refresh();
            if (_controller.Status == ServiceControllerStatus.Running)
            {
                return;
            }

            _controller.Start();
            _controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }

        public void Stop(TimeSpan timeout)
        {
            _controller.Refresh();
            if (_controller.Status == ServiceControllerStatus.Stopped)
            {
                return;
            }

            _controller.Stop();
            _controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }

        public Task Watch(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () =>
            {
                _controller.Refresh();
                _status = _controller.Status;
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    _controller.Refresh();
                    var status = _controller.Status;
                    if (status == _status)
                    {
                        continue;
                    }

                    Console.WriteLine(Name);
                    _status = status;
                    if (status != ServiceControllerStatus.Stopped)
                    {
                        continue;
                    }


                    try
                    {
                        _controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(1));
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                        Policy.Handle<InvalidEnumArgumentException>()
                            .Or<TimeoutException>()
                            .Or<Win32Exception>()
                            .Or<InvalidOperationException>()
                            .WaitAndRetry(1, i => TimeSpan.FromSeconds(i * 2))
                            .Execute(() => Start(TimeSpan.FromSeconds(1)));
                    }
                    catch (Exception)
                    {
                        _healthy = false;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}