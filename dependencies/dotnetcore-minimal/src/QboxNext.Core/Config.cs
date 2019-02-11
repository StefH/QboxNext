using System;
using System.Collections.Generic;
using System.Text;

namespace QboxNext.Core
{
    /// <summary>
    /// SAM: for now we store global configuration here, since it makes it easier to have the config
    /// platform specific. Later on we want to move this to a platform specific config file so it can
    /// also be user specific.
    /// </summary>
    public static class Config
    {
        public static string DataStorePath => Environment.OSVersion.Platform == PlatformID.Win32NT ? @"D:\QboxNextData" : "/var/qboxnextdata";
    }
}
