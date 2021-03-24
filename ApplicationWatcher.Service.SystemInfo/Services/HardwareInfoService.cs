using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using ApplicationWatcher.Service.SystemInfo.Interfaces;
using ApplicationWatcher.Service.SystemInfo.Models;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Logging;

namespace ApplicationWatcher.Service.SystemInfo.Services
{
    public class HardwareInfoService : IHardwareInfoService
    {
        private readonly ILogger<HardwareInfoService> _logger;

        public HardwareInfoService(ILogger<HardwareInfoService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<string> GetHardwareInfo()
        {
            try
            {
                _logger.LogInformation("Try initialize hardware visor");

                var computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsMotherboardEnabled = true,
                    IsControllerEnabled = true,
                    IsNetworkEnabled = true,
                    IsStorageEnabled = true
                };

                computer.Open();
                computer.Accept(new UpdateVisitor());

                _logger.LogInformation("Try get hardware info");

                var hardwareInfo = new List<string>();
                foreach (var hardware in computer.Hardware)
                {
                    hardwareInfo.Add($"Hardware: {hardware.Name}");

                    foreach (var subHardware in hardware.SubHardware)
                    {
                        hardwareInfo.Add($"\tSubHardware: {subHardware.Name}");
                        hardwareInfo.AddRange(subHardware.Sensors.Select(sensor => $"\t\tSensor: {sensor.Name}, value: {sensor.Value}"));
                    }

                    hardwareInfo.AddRange(hardware.Sensors.Select(sensor => $"\tSensor: {sensor.Name}, value: {sensor.Value}"));
                }

                computer.Close();

                return hardwareInfo;
            }
            catch (Exception e)
            {
                _logger.LogError("Error get hardware info", e);
                return null;
            }
        }

        public IReadOnlyCollection<DriverInfo> GetDriverInfo()
        {
            try
            {
                _logger.LogInformation("Try get driver info");

                using var driverSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
                return (from x in driverSearcher.Get().OfType<ManagementObject>()
                    where !string.IsNullOrEmpty(x.GetPropertyValue("DeviceName")?.ToString())
                    select new DriverInfo
                    {
                        DeviceName = x.GetPropertyValue("DeviceName")?.ToString(),
                        Version = x.GetPropertyValue("DriverVersion")?.ToString(),
                        Date = x.GetPropertyValue("DriverDate") != null ? ManagementDateTimeConverter.ToDateTime(x.GetPropertyValue("DriverDate").ToString()) : DateTime.MinValue
                    }).GroupBy(i => i.DeviceName).Select(i => i.First()).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError("Error get driver info", e);
                return null;
            }
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (var subHardware in hardware.SubHardware) subHardware.Accept(this);
            }

            public void VisitSensor(ISensor sensor) { }

            public void VisitParameter(IParameter parameter) { }
        }
    }
}
