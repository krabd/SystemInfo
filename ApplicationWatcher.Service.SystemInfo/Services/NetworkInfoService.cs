using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ApplicationWatcher.Service.SystemInfo.Helpers;
using ApplicationWatcher.Service.SystemInfo.Interfaces;
using ApplicationWatcher.Service.SystemInfo.Models.Network;

namespace ApplicationWatcher.Service.SystemInfo.Services
{
    public class NetworkInfoService : INetworkInfoService
    {
        public NetworkInfo GetNetworkInfo()
        {
            var computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            var ethernetInterface = nics.FirstOrDefault(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet && i.Name.Contains("Ethernet"));
            var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
            var ipProp = ethernetInterface?.GetIPProperties();

            var networkInfo = new NetworkInfo
            {
                HostName = computerProperties.HostName,
                DomainName = computerProperties.DomainName,
                Ip = ip,
                Mask = ipProp?.UnicastAddresses.FirstOrDefault(i => i.Address.Equals(ip))?.IPv4Mask,
                Gateaway = ipProp?.GatewayAddresses.FirstOrDefault()?.Address,
                Dns = ipProp?.DnsAddresses.FirstOrDefault(),
                IsDhcpEnabled = ipProp?.GetIPv4Properties().IsDhcpEnabled ?? false,
                NetworkInterfaces = new List<NetworkInterfaceInfo>()
            };

            foreach (var adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var properties = adapter.GetIPProperties();

                var versions = "";
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    versions = "IPv4";
                if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                {
                    if (versions.Length > 0)
                        versions += " ";
                    versions += "IPv6";
                }

                networkInfo.NetworkInterfaces.Add(new NetworkInterfaceInfo
                {
                    Description = adapter.Description,
                    NetworkInterfaceType = adapter.NetworkInterfaceType,
                    PhysicalAddress = adapter.GetPhysicalAddress(),
                    OperationalStatus = adapter.OperationalStatus,
                    IpVersion = versions,
                    DnsSuffix = properties.DnsSuffix,
                    IsDnsEnabled = properties.IsDnsEnabled,
                    IsDynamicDnsEnabled = properties.IsDynamicDnsEnabled,
                    IsReceiveOnly = adapter.IsReceiveOnly,
                    SupportsMulticast = adapter.SupportsMulticast
                });
            }

            return networkInfo;
        }

        public IpRouteTable GetRouteTable()
        {
            var fwdTable = IntPtr.Zero;
            var size = 0;
            var result = NetworkHelper.GetIpForwardTable(fwdTable, ref size, true);
            fwdTable = Marshal.AllocHGlobal(size);
            result = NetworkHelper.GetIpForwardTable(fwdTable, ref size, true);

            var forwardTable = (Ip4RouteTable.IPForwardTable)Marshal.PtrToStructure(fwdTable, typeof(Ip4RouteTable.IPForwardTable));

            var table = new Ip4RouteTable.PMIB_IPFORWARDROW[forwardTable.Size];
            var p = new IntPtr(fwdTable.ToInt64() + Marshal.SizeOf(forwardTable.Size));
            for (var i = 0; i < forwardTable.Size; ++i)
            {
                table[i] = (Ip4RouteTable.PMIB_IPFORWARDROW)Marshal.PtrToStructure(p, typeof(Ip4RouteTable.PMIB_IPFORWARDROW));
                p = new IntPtr(p.ToInt64() + Marshal.SizeOf(typeof(Ip4RouteTable.PMIB_IPFORWARDROW)));
            }
            forwardTable.Table = table;

            Marshal.FreeHGlobal(fwdTable);

            var routeTable = new IpRouteTable {RouteTable = new List<Ip4RouteEntry>()};
            for (var i = 0; i < forwardTable.Table.Length; ++i)
            {
                var entry = new Ip4RouteEntry
                {
                    DestinationIP = new IPAddress(forwardTable.Table[i].dwForwardDest),
                    SubnetMask = new IPAddress(forwardTable.Table[i].dwForwardMask),
                    GatewayIP = new IPAddress(forwardTable.Table[i].dwForwardNextHop),
                    InterfaceIndex = Convert.ToInt32(forwardTable.Table[i].dwForwardIfIndex),
                    ForwardType = Convert.ToInt32(forwardTable.Table[i].dwForwardType),
                    ForwardProtocol = Convert.ToInt32(forwardTable.Table[i].dwForwardProto),
                    ForwardAge = Convert.ToInt32(forwardTable.Table[i].dwForwardAge),
                    Metric = Convert.ToInt32(forwardTable.Table[i].dwForwardMetric1)
                };
                routeTable.RouteTable.Add(entry);
            }

            return routeTable;
        }
    }

    public class Ip4RouteTable
    {
        [ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal struct IPForwardTable
        {
            public uint Size;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public PMIB_IPFORWARDROW[] Table;
        };

        [ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal struct PMIB_IPFORWARDROW
        {
            internal uint dwForwardDest;
            internal uint dwForwardMask;
            internal uint dwForwardPolicy;
            internal uint dwForwardNextHop;
            internal uint dwForwardIfIndex;
            internal uint dwForwardType;
            internal uint dwForwardProto;
            internal uint dwForwardAge;
            internal uint dwForwardNextHopAS;
            internal uint dwForwardMetric1;
            internal uint dwForwardMetric2;
            internal uint dwForwardMetric3;
            internal uint dwForwardMetric4;
            internal uint dwForwardMetric5;
        }
    }
}
