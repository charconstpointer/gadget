﻿using Gadget.Messaging.Commands;

namespace Gadget.Inspector.HandlerRegistration
{
    public interface IHandler<T> where T : IGadgetMessage
    {
        //void Execute();

        //void Send(T message);
    }
}
