using System.Collections.Generic;

namespace Gadget.Inspector
{
    public class RequestedServices
    {
        public IEnumerable<RequestedService> Services { get; set; }
    }

    public class RequestedService
    {
        public string Name { get; set; }
        public object Config { get; set; }
    }
}