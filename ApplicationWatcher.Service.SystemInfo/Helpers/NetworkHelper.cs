using System;
using System.Runtime.InteropServices;

namespace ApplicationWatcher.Service.SystemInfo.Helpers
{
    public class NetworkHelper
    {
        [DllImport("iphlpapi", CharSet = CharSet.Auto)]
        public static extern int GetIpForwardTable(IntPtr pIpForwardTable, ref int pdwSize, bool bOrder);
    }
}
