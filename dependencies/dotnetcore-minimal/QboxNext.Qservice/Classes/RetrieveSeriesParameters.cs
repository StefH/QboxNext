using QboxNext.Core.Utils;
using System;
using QboxNext.Qbiz.Dto;

namespace QboxNext.Qservice.Classes
{
    public class RetrieveSeriesParameters
    {
        public RetrieveSeriesParameters()
        {
            OnlyQboxSolar = false;
        }
        public string QboxSerial { get; set; }
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public SeriesResolution Resolution { get; set; }
        public bool OnlyQboxSolar { get; set; }
        public EnergyType? DeviceEnergyType { get; set; }
    }
}