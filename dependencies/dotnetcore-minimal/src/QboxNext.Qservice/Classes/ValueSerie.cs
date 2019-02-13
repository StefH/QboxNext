using System.Collections.Generic;
using System.Linq;
using QboxNext.Core.Dto;
using QboxNext.Storage;

namespace QboxNext.Qservice.Classes
{
    public class ValueSerie
    {
        public string Name { get; set; }
        public DeviceEnergyType EnergyType { get; set; }
        public List<SeriesValue> Data { get; set; }
        public decimal Total { get; set; }

        protected bool Equals(ValueSerie other)
        {
            return EnergyType == other.EnergyType && Data.SequenceEqual(other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ValueSerie)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)EnergyType * 397) ^ (Data != null ? Data.GetHashCode() : 0);
            }
        }
    }
}