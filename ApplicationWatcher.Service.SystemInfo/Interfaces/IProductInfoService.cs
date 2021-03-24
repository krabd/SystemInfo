using System.Collections.Generic;
using ApplicationWatcher.Service.SystemInfo.Models;

namespace ApplicationWatcher.Service.SystemInfo.Interfaces
{
    public interface IProductInfoService
    {
        IReadOnlyCollection<ProductInfo> GetInstalledProducts();

        IReadOnlyCollection<string> GetAutoRunProducts();
    }
}
