﻿using System;
using System.ServiceProcess;
using System.Threading.Channels;
using System.Threading.Tasks;
using Gadget.Messaging;

namespace Gadget.Inspector
{
    internal class WindowsService : IWindowsService
    {
        private readonly ServiceController _serviceController;
        private ServiceControllerStatus _lastKnownStatus;
        private readonly ChannelWriter<ServiceStatusChanged> _channelWriter;
        public void Start() => _serviceController.Start();

        public void Stop() => _serviceController.Stop();

        public WindowsService(ServiceController serviceController, ChannelWriter<ServiceStatusChanged> channelWriter)
        {
            _serviceController = serviceController;
            _channelWriter = channelWriter;
            StartWatcher();
        }

        public ServiceControllerStatus Status
        {
            get
            {
                _serviceController.Refresh();
                var currentStatus = _serviceController.Status;
                _lastKnownStatus = currentStatus;
                return currentStatus;
            }
        }


        //TODO Replace this with global scheduler/watcher that utilizes less resources? 
        private void StartWatcher()
        {
            //Possibly stealing thread from thread pool and never returning it
            var _ = Task.Run(async () =>
            {
                while (true)
                {
                    _serviceController.Refresh();
                    var currentStatus = _serviceController.Status;
                    if (currentStatus != _lastKnownStatus)
                    {
                        var change = new ServiceStatusChanged
                        {
                            Name = _serviceController.ServiceName,
                            Status = Status.ToString()
                        };
                        await _channelWriter.WriteAsync(change);
                    }

                    _lastKnownStatus = currentStatus;
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                // ReSharper disable once FunctionNeverReturns
            });
        }
    }
}