using System.Net;
using Newtonsoft.Json;

namespace ApplicationWatcher.Service.SystemInfo.Models.Network
{
    public class Ip4RouteEntry
    {
        public string DestinationIPString => DestinationIP?.ToString();

        public string SubnetMaskString => SubnetMask?.ToString();

        public string GatewayIPString => GatewayIP?.ToString();

        public int InterfaceIndex { get; set; }

        public int ForwardType { get; set; }

        public int ForwardProtocol { get; set; }

        public int ForwardAge { get; set; }

        public int Metric { get; set; }

        [JsonIgnore]
        public IPAddress DestinationIP { get; set; }

        [JsonIgnore]
        public IPAddress SubnetMask { get; set; }

        [JsonIgnore]
        public IPAddress GatewayIP { get; set; }
    }

}
