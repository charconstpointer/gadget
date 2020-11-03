﻿using System;
using System.Collections.Generic;

namespace Gadget.Inspector.Models
{
    public class MachineHealthDataModel
    {
        public Guid MachineId { get; set; }

        public string MachineName { get; set; }

        public int CpuPercentUsage { get; set; }

        public int ProcessesQuantity { get; set; }

        public int CpuThreadsQuantity { get; set; }

        public float MemoryFree { get; set; }
        public float MemoryTotal { get; set; }

        public string Platform { get; set; }

        public List<DiscUsageInfo> Discs { get; set; }
    }

    public class DiscUsageInfo
    {
        public string Name { get; set; }

        public float DiscSpaceFree { get; set; }

        public float DiscSize { get; set; }
    }
}
