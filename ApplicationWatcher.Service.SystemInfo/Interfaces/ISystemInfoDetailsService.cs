using System.Collections.Generic;
using ApplicationWatcher.Service.SystemInfo.Models;
using ApplicationWatcher.Service.SystemInfo.Models.Hdd;
using ApplicationWatcher.Service.SystemInfo.Models.Network;

namespace ApplicationWatcher.Service.SystemInfo.Interfaces
{
    public interface ISystemInfoDetailsService
    {
        OperationSystemInfo GetOperationSystemInfo();

        //TODO: Current user
        IReadOnlyCollection<ProductInfo> GetInstalledProducts();

        //TODO: Current user
        IReadOnlyCollection<string> GetAutoRunProducts();

        IReadOnlyCollection<EventLogInfo> GetEventLogs();

        IReadOnlyCollection<string> GetHardwareInfo();

        IReadOnlyCollection<DriverInfo> GetDriverInfo();

        IReadOnlyCollection<VideoInfo> GetVideoInfo();

        NetworkInfo GetNetworkInfo();

        IReadOnlyCollection<UserInfo> GetUsersInfo();

        IReadOnlyCollection<DiskInfo> GetDiskInfo();

        IReadOnlyCollection<string> GetOperationSystemUpdatesInfo();
    }
}
