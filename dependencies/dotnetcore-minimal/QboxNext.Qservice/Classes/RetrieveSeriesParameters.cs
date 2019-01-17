using QboxNext.Core.Utils;
using System;
using QboxNext.Qbiz.Dto;

namespace QboxNext.Qservice.Classes
{
    public class RetrieveSeriesParameters
    {
        public string QboxSerial { get; set; }
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public SeriesResolution Resolution { get; set; }
        public EnergyType? DeviceEnergyType { get; set; }
    }
}