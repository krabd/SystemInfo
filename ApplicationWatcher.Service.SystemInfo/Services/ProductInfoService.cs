using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationWatcher.Service.SystemInfo.Interfaces;
using ApplicationWatcher.Service.SystemInfo.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace ApplicationWatcher.Service.SystemInfo.Services
{
    public class ProductInfoService : IProductInfoService
    {
        private readonly ILogger<ProductInfoService> _logger;

        public ProductInfoService(ILogger<ProductInfoService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<ProductInfo> GetInstalledProducts()
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);

                _logger.LogInformation($"Try find programs in 32");

                var installedProducts32 = new List<ProductInfo>();
                using var key32 = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);
                GetProducts(key32, ref installedProducts32);

                _logger.LogInformation($"Try find programs in 64");

                var installedProducts64 = new List<ProductInfo>();
                using var key64 = baseKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false);
                GetProducts(key64, ref installedProducts64);

                var installedProducts = installedProducts32.Union(installedProducts64).GroupBy(i => i.Name).Select(g => g.First()).ToList();
                return installedProducts;

                void GetProducts(RegistryKey key, ref List<ProductInfo> installedProductsRef)
                {
                    if (key != null)
                    {
                        foreach (var productKeyName in key.GetSubKeyNames())
                        {
                            using var productKey = key.OpenSubKey(productKeyName, false);
                            if (productKey != null)
                            {
                                var name = (string)productKey.GetValue("DisplayName");
                                var releaseType = (string)productKey.GetValue("ReleaseType");
                                var systemComponent = productKey.GetValue("SystemComponent");
                                var parentName = (string)productKey.GetValue("ParentDisplayName");
                                if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(releaseType) && string.IsNullOrEmpty(parentName) && systemComponent == null)
                                {
                                    installedProductsRef.Add(new ProductInfo
                                    {
                                        Name = name,
                                        Version = productKey.GetValue("DisplayVersion") as string
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error find installed products", e);
                return null;
            }
        }

        public IReadOnlyCollection<string> GetAutoRunProducts()
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);

                _logger.LogInformation($"Try find auto run programs in 32");

                var autoRunProducts32 = new List<string>();
                using var key32 = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                GetProducts(key32, ref autoRunProducts32);

                _logger.LogInformation($"Try find auto run  programs in 64");

                var autoRunProducts64 = new List<string>();
                using var key64 = baseKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run", false);
                GetProducts(key64, ref autoRunProducts64);

                var autoRunProducts = autoRunProducts32.Union(autoRunProducts64).GroupBy(i => i).Select(g => g.First()).ToList();
                return autoRunProducts;

                void GetProducts(RegistryKey key, ref List<string> autoRunProductsRef)
                {
                    if (key != null)
                    {
                        autoRunProductsRef.AddRange(key.GetValueNames());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error find autorun products", e);
                return null;
            }
        }
    }
}
