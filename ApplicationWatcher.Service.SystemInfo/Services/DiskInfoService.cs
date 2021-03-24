using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using ApplicationWatcher.Service.SystemInfo.Interfaces;
using ApplicationWatcher.Service.SystemInfo.Models.Hdd;
using Microsoft.Extensions.Logging;

namespace ApplicationWatcher.Service.SystemInfo.Services
{
    public class DiskInfoService : IDiskInfoService
    {
        private readonly ILogger<DiskInfoService> _logger;

        public DiskInfoService(ILogger<DiskInfoService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<DiskInfo> GetDiskInfo()
        {
            var dicDrives = new Dictionary<int, DiskInfo>();

            try
            {
                using var wdSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                var iDriveIndex = 0;
                foreach (var drive in wdSearcher.Get())
                {
                    var hdd = new DiskInfo
                    {
                        Model = drive["Model"].ToString(), 
                        Type = drive["InterfaceType"].ToString()
                    };
                    dicDrives.Add(iDriveIndex, hdd);
                    iDriveIndex++;
                }

                using var pmSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

                // retrieve hdd serial number
                iDriveIndex = 0;
                foreach (var drive in pmSearcher.Get())
                {
                    // because all physical media will be returned we need to exit
                    // after the hard drives serial info is extracted
                    if (iDriveIndex >= dicDrives.Count)
                        break;

                    dicDrives[iDriveIndex].Serial = drive["SerialNumber"] == null ? "None" : drive["SerialNumber"].ToString().Trim();
                    iDriveIndex++;
                }

                using var searcher = new ManagementObjectSearcher("SELECT * from Win32_DiskDrive");
                searcher.Scope = new ManagementScope(@"\root\wmi");

                // check if SMART reports the drive is failing
                searcher.Query = new ObjectQuery("SELECT * from MSStorageDriver_FailurePredictStatus");
                iDriveIndex = 0;
                foreach (var drive in searcher.Get())
                {
                    dicDrives[iDriveIndex].IsOK = (bool)drive.Properties["PredictFailure"].Value == false;
                    iDriveIndex++;
                }

                // retrive attribute flags, value worste and vendor data information
                searcher.Query = new ObjectQuery("SELECT * from MSStorageDriver_FailurePredictData");
                iDriveIndex = 0;
                foreach (var data in searcher.Get())
                {
                    var bytes = (byte[])data.Properties["VendorSpecific"].Value;
                    for (var i = 0; i < 30; ++i)
                    {
                        try
                        {
                            int id = bytes[i * 12 + 2];

                            int flags = bytes[i * 12 + 4]; // least significant status byte, +3 most significant byte, but not used so ignored.
                                                           //bool advisory = (flags & 0x1) == 0x0;
                            var failureImminent = (flags & 0x1) == 0x1;
                            //bool onlineDataCollection = (flags & 0x2) == 0x2;

                            int value = bytes[i * 12 + 5];
                            int worst = bytes[i * 12 + 6];
                            var vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                            if (id == 0) continue;

                            var attr = dicDrives[iDriveIndex].Attributes[id];
                            attr.Current = value;
                            attr.Worst = worst;
                            attr.Data = vendordata;
                            attr.IsOK = failureImminent == false;
                        }
                        catch
                        {
                            // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                        }
                    }
                    iDriveIndex++;
                }

                // retreive threshold values foreach attribute
                searcher.Query = new ObjectQuery("SELECT * from MSStorageDriver_FailurePredictThresholds");
                iDriveIndex = 0;
                foreach (var data in searcher.Get())
                {
                    var bytes = (byte[])data.Properties["VendorSpecific"].Value;
                    for (var i = 0; i < 30; ++i)
                    {
                        try
                        {
                            int id = bytes[i * 12 + 2];
                            int thresh = bytes[i * 12 + 3];
                            if (id == 0) continue;

                            var attr = dicDrives[iDriveIndex].Attributes[id];
                            attr.Threshold = thresh;
                        }
                        catch
                        {
                            // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                        }
                    }

                    iDriveIndex++;
                }
                
            }
            catch (ManagementException e)
            {
                _logger.LogError("Error find disk info", e);
            }

            return dicDrives.Values.ToList();
        }
    }
}
