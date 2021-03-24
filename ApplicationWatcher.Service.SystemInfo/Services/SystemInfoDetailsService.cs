using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using ApplicationWatcher.Service.SystemInfo.Interfaces;
using ApplicationWatcher.Service.SystemInfo.Models;
using ApplicationWatcher.Service.SystemInfo.Models.Hdd;
using ApplicationWatcher.Service.SystemInfo.Models.Network;
using ApplicationWatcher.Service.Utils.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApplicationWatcher.Service.SystemInfo.Services
{
    public class SystemInfoDetailsService : ISystemInfoDetailsService
    {
        private readonly IRegistryService _registryService;
        private readonly IProductInfoService _productInfoService;
        private readonly IHardwareInfoService _hardwareInfoService;
        private readonly INetworkInfoService _networkInfoService;
        private readonly IDiskInfoService _diskInfoService;
        private readonly ILogger<SystemInfoDetailsService> _logger;

        public SystemInfoDetailsService(IRegistryService registryService, IProductInfoService productInfoService, IHardwareInfoService hardwareInfoService, INetworkInfoService networkInfoService, IDiskInfoService diskInfoService,
            ILogger<SystemInfoDetailsService> logger)
        {
            _registryService = registryService;
            _productInfoService = productInfoService;
            _hardwareInfoService = hardwareInfoService;
            _networkInfoService = networkInfoService;
            _diskInfoService = diskInfoService;
            _logger = logger;
        }

        public OperationSystemInfo GetOperationSystemInfo()
        {
            using var operatingSystemSearcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            var name = (from x in operatingSystemSearcher.Get().OfType<ManagementObject>()
                select x.GetPropertyValue("Caption").ToString()).FirstOrDefault();

            return new OperationSystemInfo
            {
                Name = name ?? "Unknown",
                Build = _registryService.GetRegistryValue<string>(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild"),
                Ubr = _registryService.GetRegistryValue<int>(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "UBR")
            };
        }

        public IReadOnlyCollection<ProductInfo> GetInstalledProducts()
        {
            return _productInfoService.GetInstalledProducts();
        }

        public IReadOnlyCollection<string> GetAutoRunProducts()
        {
            return _productInfoService.GetAutoRunProducts();
        }

        public IReadOnlyCollection<EventLogInfo> GetEventLogs()
        {
            _logger.LogInformation("Try get security event logs");

            var securityLog = new EventLog("Security").Entries.Cast<EventLogEntry>()
                .Select(x => new EventLogInfo
                {
                    MachineName = x.MachineName,
                    Source = x.Source,
                    Message = x.Message
                }).ToList();

            _logger.LogInformation("Try get system event logs");

            var systemLog = new EventLog("System").Entries.Cast<EventLogEntry>()
                .Select(x => new EventLogInfo
                {
                    MachineName = x.MachineName,
                    Source = x.Source,
                    Message = x.Message
                }).ToList();

            _logger.LogInformation("Try get application event logs");

            var applicationLog = new EventLog("Application").Entries.Cast<EventLogEntry>()
                .Select(x => new EventLogInfo
                {
                    MachineName = x.MachineName,
                    Source = x.Source,
                    Message = x.Message
                }).ToList();

            return securityLog.Union(systemLog).Union(applicationLog).ToList();
        }

        public IReadOnlyCollection<string> GetHardwareInfo()
        {
            return _hardwareInfoService.GetHardwareInfo();
        }

        public IReadOnlyCollection<DriverInfo> GetDriverInfo()
        {
            return _hardwareInfoService.GetDriverInfo();
        }

        public IReadOnlyCollection<VideoInfo> GetVideoInfo()
        {
            try
            {
                _logger.LogInformation("Try get video info");

                using var videoControllerSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                return (from x in videoControllerSearcher.Get().OfType<ManagementObject>()
                    select new VideoInfo
                    {
                        AdapterCompatibility = x.GetPropertyValue("AdapterCompatibility").ToString(),
                        Caption = x.GetPropertyValue("Caption").ToString(),
                        ResolutionWidth = Convert.ToInt32(x.GetPropertyValue("CurrentHorizontalResolution")),
                        ResolutionHeight = Convert.ToInt32(x.GetPropertyValue("CurrentVerticalResolution")),
                        BitsPerPixel = Convert.ToInt32(x.GetPropertyValue("CurrentBitsPerPixel")),
                        RefreshRate = Convert.ToInt32(x.GetPropertyValue("CurrentRefreshRate"))
                    }).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError("Error get video info", e);
                return null;
            }
        }

        public NetworkInfo GetNetworkInfo()
        {
            _logger.LogInformation("Try get network info");

            var networkInfo = _networkInfoService.GetNetworkInfo();
            networkInfo.RouteTable = _networkInfoService.GetRouteTable();
            return networkInfo;
        }

        public IReadOnlyCollection<UserInfo> GetUsersInfo()
        {
            _logger.LogInformation("Try get users info");

            using var usersSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount");
            return usersSearcher.Get().OfType<ManagementObject>().Select(x =>
            {
                var user = new UserInfo
                {
                    Description = x.GetPropertyValue("Description").ToString(),
                    IsDisabled = Convert.ToBoolean(x.GetPropertyValue("Disabled")),
                    Domain = x.GetPropertyValue("Domain").ToString(),
                    FullName = x.GetPropertyValue("FullName").ToString(),
                    IsLocalAccount = Convert.ToBoolean(x.GetPropertyValue("LocalAccount")),
                    IsLockout = Convert.ToBoolean(x.GetPropertyValue("Lockout")),
                    Name = x.GetPropertyValue("Name").ToString(),
                    Sid = x.GetPropertyValue("SID").ToString(),
                    SidType = (SidType) Convert.ToInt32(x.GetPropertyValue("SIDType")),
                    Status = x.GetPropertyValue("Status").ToString(),
                };

                var partComponent = $"Win32_UserAccount.Domain='{user.Domain}',Name='{user.Name}'";
                var query = new ObjectQuery("SELECT * FROM Win32_GroupUser WHERE PartComponent = \"" + partComponent + "\"");
                using var groupSearcher = new ManagementObjectSearcher(query);
                user.GroupName = groupSearcher.Get().OfType<ManagementObject>().Select(i =>
                {
                    var groupComponent = new ManagementObject(i.GetPropertyValue("GroupComponent").ToString());
                    return groupComponent.GetPropertyValue("Name").ToString();
                }).FirstOrDefault();

                return user;
            }).ToList();
        }

        public IReadOnlyCollection<DiskInfo> GetDiskInfo()
        {
            _logger.LogInformation("Try get disk info");

            return _diskInfoService.GetDiskInfo();
        }

        public IReadOnlyCollection<string> GetOperationSystemUpdatesInfo()
        {
            var updates = new List<string>();

            //var uSession = new UpdateSession();
            //var uSearcher = uSession.CreateUpdateSearcher();
            //uSearcher.Online = false;

            //try
            //{
            //    var sResult = uSearcher.Search("IsInstalled=1 And IsHidden=0");
            //    updates.AddRange(from IUpdate update in sResult.Updates select update.Title);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError("Something went wrong: " + ex.Message);
            //}

            return updates;
        }
    }
}
