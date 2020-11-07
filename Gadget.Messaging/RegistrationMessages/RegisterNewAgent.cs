﻿using System;
using System.Collections.Generic;

namespace Gadget.Messaging
{
    public class RegisterNewAgent
    {
        public string Machine { get; set; }
        public Guid AgentId { get; set; }
        public IEnumerable<Service> Services { get; set; }
    }
}