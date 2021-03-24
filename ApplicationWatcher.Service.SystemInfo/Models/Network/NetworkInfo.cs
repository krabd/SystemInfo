using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace ApplicationWatcher.Service.SystemInfo.Models.Network
{
    public class NetworkInfo
    {
        public string HostName { get; set; }

        public string DomainName { get; set; }

        public string IpString => Ip?.ToString();

        public string MaskString => Mask?.ToString();

        public string GateawayString => Gateaway?.ToString();

        public string DnsString => Dns?.ToString();

        public bool IsDhcpEnabled { get; set; }

        public IpRouteTable RouteTable { get; set; }

        public List<NetworkInterfaceInfo> NetworkInterfaces { get; set; }

        [JsonIgnore]
        public IPAddress Ip { get; set; }

        [JsonIgnore]
        public IPAddress Mask { get; set; }

        [JsonIgnore]
        public IPAddress Gateaway { get; set; }

        [JsonIgnore]
        public IPAddress Dns { get; set; }
    }
}
