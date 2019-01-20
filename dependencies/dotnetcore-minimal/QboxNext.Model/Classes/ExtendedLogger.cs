using System;

namespace QboxNext.Model.Classes
{
    public class ExtendedLogger : IDisposable
    {
        public ExtendedLogger(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber)) throw new ArgumentNullException("serialNumber");
            NLog.MappedDiagnosticsContext.Set("sn", serialNumber);
        }

        public void Dispose()
        {
            if (NLog.MappedDiagnosticsContext.Contains("sn"))
                NLog.MappedDiagnosticsContext.Remove("sn");
        }
    }
}
