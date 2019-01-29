using QboxNext.Core.Utils;
using System;
using QboxNext.Qbiz.Dto;
using QboxNext.Model.Qboxes;

namespace QboxNext.Qservice.Classes
{
    public class RetrieveSeriesParameters
    {
        public Mini Mini { get; set; }
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public SeriesResolution Resolution { get; set; }
        public EnergyType? DeviceEnergyType { get; set; }
    }
}