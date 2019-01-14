using System;
using System.Collections.Generic;
using System.Linq;
using QboxNext.Core.Dto;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Statistics;

namespace QboxNext.Qservice.Classes
{
    public class ValueSerie
    {
        public string Name { get; set; }
        public DeviceEnergyType EnergyType { get; set; }
        public List<SeriesValue> Data { get; set; }
        public decimal Total { get; set; }
        public decimal TotalMoney { get; set; }
        public decimal RelativeTotal { get; set; }

        public ValueSerie GetAsMonthlyResolution()
        {
            var monthlyResolutionSeriesValues = Data.Aggregate(new List<SeriesValue>(),
                (result, seriesValue) =>
                {
                    if (!result.Any() || !result.Last().Begin.InSameMonth(seriesValue.Begin))
                    {
                        var newSeriesValueBegin = new DateTime(seriesValue.Begin.Year, seriesValue.Begin.Month, 1);
                        var newSeriesValueEnd = newSeriesValueBegin.AddMonths(1);
                        result.Add(new SeriesValue(newSeriesValueBegin, newSeriesValueEnd, seriesValue.Value.GetValueOrDefault()));
                        return result;
                    }

                    var runningMonthSeriesValue = result.Last();
                    result.Remove(runningMonthSeriesValue);
                    result.Add(new SeriesValue(runningMonthSeriesValue.Begin, runningMonthSeriesValue.End,
                        runningMonthSeriesValue.Value.GetValueOrDefault() + seriesValue.Value.GetValueOrDefault()));
                    return result;
                });
            return new ValueSerie
            {
                Name = Name,
                EnergyType = EnergyType,
                Data = monthlyResolutionSeriesValues,
                Total = monthlyResolutionSeriesValues.Sum(x => x.Value.GetValueOrDefault()),
                RelativeTotal = RelativeTotal,
                TotalMoney = TotalMoney
            };
        }

        /// <summary>
        /// When Serie has a null value, the serie will not be taken into account
        /// by the graph, so all the data shifts to the left and values do not appear
        /// at the correct date. Therefore always use GetValueOrDefault.
        /// </summary>
        /// <returns></returns>
        public Serie GetAsSerieForGraph()
        {
            return new Serie
            {
                Data = Data.Select(x => x.Value.GetValueOrDefault()).Cast<decimal?>().ToList(),
                EnergyType = EnergyType,
                Name = Name,
                RelativeTotal = RelativeTotal,
                Total = Total,
                TotalMoney = TotalMoney
            };
        }

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