using System.Collections.Generic;
using ApplicationWatcher.Service.SystemInfo.Models.Hdd;
using ApplicationWatcher.Service.SystemInfo.Models.Network;

namespace ApplicationWatcher.Service.SystemInfo.Models
{
    public class SystemLog
    {
        public OperationSystemInfo OperationSystemInfo { get; set; }

        public IReadOnlyCollection<ProductInfo> InstalledProducts { get; set; }

        public IReadOnlyCollection<string> AutoRunProducts { get; set; }

        public IReadOnlyCollection<EventLogInfo> EventLogs { get; set; }

        public IReadOnlyCollection<string> HardwareInfo { get; set; }

        public IReadOnlyCollection<DriverInfo> DriverInfo { get; set; }

        public IReadOnlyCollection<VideoInfo> VideoInfo { get; set; }

        public NetworkInfo NetworkInfo { get; set; }

        public IReadOnlyCollection<UserInfo> UsersInfo { get; set; }

        public IReadOnlyCollection<DiskInfo> DiskInfo { get; set; }

        public IReadOnlyCollection<string> OperationSystemUpdatesInfo { get; set; }

        public TimeInfo TimeInfo { get; set; }
    }
}
