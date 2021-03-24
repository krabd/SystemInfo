using ApplicationWatcher.Service.SystemInfo.Models.Network;

namespace ApplicationWatcher.Service.SystemInfo.Interfaces
{
    public interface INetworkInfoService
    {
        NetworkInfo GetNetworkInfo();

        IpRouteTable GetRouteTable();
    }
}
