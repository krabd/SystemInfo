using System.Collections.Generic;
using ApplicationWatcher.Service.SystemInfo.Models;

namespace ApplicationWatcher.Service.SystemInfo.Interfaces
{
    public interface IHardwareInfoService
    {
        IReadOnlyCollection<string> GetHardwareInfo();

        IReadOnlyCollection<DriverInfo> GetDriverInfo();
    }
}
