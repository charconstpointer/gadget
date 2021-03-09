using System;
using Gadget.Notifications.Domain.Enums;

namespace Gadget.Notifications.Domain.ValueObjects
{
    public class Receiver : IReceiver
    {
        public DateTime CreatedAt { get; }
        public NotifierType NotifierType { get; }
        public string Destination { get; }

        private Receiver()
        {
            
        }
        public Receiver(string agentName, string serviceName, string destination, NotifierType notifierType)
        {
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
            NotifierType = notifierType;
            CreatedAt = DateTime.UtcNow;
        }
    }

    public interface IReceiver
    {
        string Destination { get; }
    }
}