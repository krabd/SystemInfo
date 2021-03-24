using System.Collections.Generic;
using ApplicationWatcher.Service.SystemInfo.Models.Hdd;

namespace ApplicationWatcher.Service.SystemInfo.Interfaces
{
    public interface IDiskInfoService
    {
        IReadOnlyCollection<DiskInfo> GetDiskInfo();
    }
}
