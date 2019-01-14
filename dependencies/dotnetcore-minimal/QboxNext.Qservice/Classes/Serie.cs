using QboxNext.Core.Dto;
using System.Collections.Generic;

namespace QboxNext.Qservice.Classes
{
    public class SerieBase
    {
        public string Name { get; set; }
        public DeviceEnergyType EnergyType { get; set; }
        public decimal Total { get; set; }
        public decimal TotalMoney { get; set; }
        public decimal RelativeTotal { get; set; }
        //public decimal DataAverage { get; set; }

        internal void AddTotals(decimal tariff, decimal usageFactor, decimal totalUsage)
        {
            var factoredUsage = totalUsage / usageFactor;
            Total += factoredUsage;
            TotalMoney += factoredUsage / 1000m * tariff; // TODO extract 1000m (conversion to kWh from Wh)
        }
    }

    public class Serie : SerieBase
    {
        public IList<decimal?> Data { get; set; }
    }
}