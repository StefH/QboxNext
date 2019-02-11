using QboxNext.Core.Dto;
using System.Collections.Generic;

namespace QboxNext.Qservice.Classes
{
    public class SerieBase
    {
        public string Name { get; set; }
        public DeviceEnergyType EnergyType { get; set; }
        public decimal Total { get; set; }
        public decimal RelativeTotal { get; set; }
    }

    public class Serie : SerieBase
    {
        public IList<decimal?> Data { get; set; }
    }
}