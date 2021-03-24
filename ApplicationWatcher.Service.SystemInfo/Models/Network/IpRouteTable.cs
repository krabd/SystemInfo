using System.Collections.Generic;

namespace ApplicationWatcher.Service.SystemInfo.Models.Network
{
    public class IpRouteTable
    {
        public List<Ip4RouteEntry> RouteTable { get; set; }
    }
}
