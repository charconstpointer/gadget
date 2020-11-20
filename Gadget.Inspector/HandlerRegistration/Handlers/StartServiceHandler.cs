﻿using Gadget.Messaging.Commands;

namespace Gadget.Inspector.HandlerRegistration.Handlers
{
    class StartServiceHandler : IHandler<StartService>
    {
        public void StartService()
        {
            System.Console.WriteLine("StartServiceHandler works");
        }
    
    }
}
