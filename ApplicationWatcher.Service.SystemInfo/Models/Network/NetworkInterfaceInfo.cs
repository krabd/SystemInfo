using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace ApplicationWatcher.Service.SystemInfo.Models.Network
{
    public class NetworkInterfaceInfo
    {
        public string Description { get; set; }

        public NetworkInterfaceType NetworkInterfaceType { get; set; }

        public string PhysicalAddressString => PhysicalAddress?.ToString();

        public OperationalStatus OperationalStatus { get; set; }

        public string IpVersion { get; set; }

        public string DnsSuffix { get; set; }

        public bool IsDnsEnabled { get; set; }

        public bool IsDynamicDnsEnabled { get; set; }

        public bool IsReceiveOnly { get; set; }

        public bool SupportsMulticast { get; set; }

        [JsonIgnore]
        public PhysicalAddress PhysicalAddress { get; set; }
    }
}
