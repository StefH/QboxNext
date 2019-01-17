using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Qboxes;
using Qboxes.Model.Qboxes;
using QboxNext.Core;
using QboxNext.Core.Dto;
using QboxNext.Core.Utils;
using QboxNext.Qbiz.Dto;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qservice.Classes;
using QboxNext.Qserver.Core.Statistics;
using QboxNext.Qservice.Logging;

namespace QboxNext.Qservice.Classes
{
    /// <summary>
    /// Class to retrieve series from QBX files.
    /// </summary>
    public class SeriesRetriever : ISeriesRetriever
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private const int GenerationCounterId = 9999;

        static SeriesRetriever()
        {
            Mapper.Initialize(cfg =>
                cfg.CreateMap<ValueSerie, Serie>()
                    .ForMember(dest => dest.Data, action => action.MapFrom(src => src.Data.Select(
                        s => s.Value).ToList()))
            );
        }

        /// <summary>
        /// Build the C# result that can be used to generate the Json result for GetSeries.
        /// </summary>
        public IList<Serie> RetrieveForAccount(string inQboxSerial, DateTime inFromUtc, DateTime inToUtc, SeriesResolution inResolution)
        {
            var valueSeries = RetrieveSerieValuesForAccount(inQboxSerial, inFromUtc, inToUtc, inResolution);
            return Mapper.Map<IEnumerable<ValueSerie>, IList<Serie>>(valueSeries);
        }


        /// <summary>
        /// Build the C# result that can be used to generate the Json result for GetSeries.
        /// </summary>
        private IEnumerable<ValueSerie> RetrieveSerieValuesForAccount(string inQboxSerial, DateTime inFromUtc, DateTime inToUtc, SeriesResolution inResolution)
        {
            var parameters = new RetrieveSeriesParameters { QboxSerial = inQboxSerial, FromUtc = inFromUtc, ToUtc = inToUtc, Resolution = inResolution };
            List<ValueSerie> resultSeries = RetrieveQboxSeries(parameters);
            // If there is generation, it can be that there are odd negative consumption numbers, these will be set to 0.
            if (resultSeries.Any(s => s.EnergyType == DeviceEnergyType.Generation) && resultSeries.Any(s => s.EnergyType == DeviceEnergyType.Consumption))
            {
                foreach (var datum in resultSeries.First(s => s.EnergyType == DeviceEnergyType.Consumption).Data.Where(d => d.Value.HasValue && d.Value < 0))
                {
                    datum.Value = 0;
                }
            }
            return resultSeries;
        }

        public List<ValueSerie> RetrieveQboxSeries(RetrieveSeriesParameters parameters)
        {
            var resultSeries = new List<ValueSerie>();
            Dictionary<int, IList<SeriesValue>> counterSeriesList = GetSeriesAtCounterLevel(parameters);
            var usageEnergyType = DeviceEnergyType.Consumption;

            foreach (var (counterId, counterSeries) in counterSeriesList)
            {
                var valueSerie = new ValueSerie
                {
                    Data = counterSeries.ToList()
                };
                valueSerie.Total = valueSerie.Data.Sum(d => d.Value ?? 0);

                switch (counterId)
                {
                    case 181: // consumption low
                        valueSerie.EnergyType = DeviceEnergyType.NetLow;
                        AddSerieToResult(valueSerie, resultSeries, usageEnergyType);
                        break;
                    case 182: // consumption high
                        valueSerie.EnergyType = DeviceEnergyType.NetHigh;
                        AddSerieToResult(valueSerie, resultSeries, usageEnergyType);
                        break;
                    case 281: // generation low
                        valueSerie.EnergyType = DeviceEnergyType.NetLow;
                        valueSerie.Data.ForEach(d => d.Value *= -1);
                        AddSerieToResult(valueSerie, resultSeries, usageEnergyType);
                        break;
                    case 282: // generation high
                        valueSerie.EnergyType = DeviceEnergyType.NetHigh;
                        valueSerie.Data.ForEach(d => d.Value *= -1);
                        AddSerieToResult(valueSerie, resultSeries, usageEnergyType);
                        break;
                    case 2421: // gas
                        if (valueSerie.Data.Sum(d => d.Value ?? 0) == 0 && valueSerie.Data.Any(d => !d.Value.HasValue))
                        {
                            break; //there is a gas counter but no data found or file found, so not monitored
                        }
                        valueSerie.EnergyType = DeviceEnergyType.Gas;
                        AddSerieToResult(valueSerie, resultSeries, null);
                        break;
                    case 1: // net but also generation???
                        valueSerie.EnergyType = usageEnergyType;
                        AddSerieToResult(valueSerie, resultSeries, null);
                        break;
                    case 3: //net
                        valueSerie.EnergyType = usageEnergyType;
                        valueSerie.Data.ForEach(d => d.Value *= -1);
                        AddSerieToResult(valueSerie, resultSeries, null);
                        break;
                    case GenerationCounterId:   //this is a fake counter that has been built to contain S0 (generation) data, 
                                                //it is set in GetSeriesAtCounterLevel() at the top of that function
                        valueSerie.EnergyType = DeviceEnergyType.Generation;
                        AddSerieToResult(valueSerie, resultSeries, null);
                        break;
                    default:
                        Log.Error("Invalid counter id :" + counterId);
                        break;
                }
            }
            return resultSeries;
        }

        private static void AddSerieToResult(ValueSerie inSerie, List<ValueSerie> ioResultSeries, DeviceEnergyType? inSecondaryEnergyType)
        {
            if (inSerie.Data == null)
                return;

            if (ioResultSeries.Any(s => s.EnergyType == inSerie.EnergyType))
            {
                var existingSerie = ioResultSeries.First(s => s.EnergyType == inSerie.EnergyType);
                var serieIndexFinder = new SerieIndexFinder(existingSerie.Data);
                foreach (var slotToAdd in inSerie.Data)
                {
                    var index = serieIndexFinder.GetIndex(slotToAdd.Begin);
                    if (index < 0)
                    {
                        existingSerie.Data.Add(slotToAdd);
                    }
                    else
                    {
                        var slot = existingSerie.Data[index];
                        if (!slot.Value.HasValue || slot.Value == 0)
                        {
                            slot.Value = slotToAdd.Value;
                        }
                        else
                        {
                            slot.Value += slotToAdd.Value ?? 0;
                        }
                    }
                }

                existingSerie.Total = existingSerie.Data.Sum(d => d.Value ?? 0);
            }
            else
            {
                ioResultSeries.Add(inSerie);
            }

            // used to accumulate series to one type:
            if (inSecondaryEnergyType.HasValue)
            {
                var serie = new ValueSerie()
                {
                    EnergyType = inSecondaryEnergyType.Value,
                    Name = inSerie.Name,
                    Data = new List<SeriesValue>()
                };
                foreach (var data in inSerie.Data)
                {
                    var seriesValue = new SeriesValue()
                    {
                        Begin = new DateTime(data.Begin.Ticks),
                        End = new DateTime(data.End.Ticks),
                        Value = data.Value
                    };
                    serie.Data.Add(seriesValue);
                }
                serie.Total = serie.Data.Sum(d => d.Value ?? 0);
                AddSerieToResult(serie, ioResultSeries, null);
            }
        }


        private static Dictionary<int, IList<SeriesValue>> GetSeriesAtCounterLevel(RetrieveSeriesParameters parameters)
        {
            var countersSeriesValue = new Dictionary<int, IList<SeriesValue>>();
            var fromNl = DateTimeUtils.UtcDateTimeToNl(parameters.FromUtc);
            var toNl = DateTimeUtils.UtcDateTimeToNl(parameters.ToUtc);

            var mini = CreateMini(parameters.QboxSerial);

            try
            {
                foreach (var counter in mini.Counters)
                {
                    try
                    {
                        foreach (var cdm in counter.CounterDeviceMappings.Where(cdm => cdm.PeriodeEind > fromNl).OrderBy(cdm => cdm.PeriodeBegin))
                        {
                            var counterId = counter.CounterId;

                            if (parameters.DeviceEnergyType.HasValue &&
                                (
                                    (parameters.DeviceEnergyType == EnergyType.Gas && counterId != 2421) ||
                                    (parameters.DeviceEnergyType == EnergyType.Electricity && counterId == 2421)
                                ))
                            {
                                continue;
                            }

                            // for generation we create a new counter so the values will be put in separate value lists
                            if (counterId == 1 && cdm.Device.EnergyType == DeviceEnergyType.Generation)
                            {
                                counterId = GenerationCounterId;
                            }
                            var actualFrom = fromNl <= cdm.PeriodeBegin ? cdm.PeriodeBegin : fromNl;
                            var actualTo = toNl > cdm.PeriodeEind ? cdm.PeriodeEind : toNl;
                            var serieValue = actualFrom < actualTo
                                ? counter.GetSeries(Unit.kWh, actualFrom, actualTo, parameters.Resolution, false, false).ToList()
                                : new List<SeriesValue>();

                            if ((serieValue != null) && (serieValue.Any()))
                            {
                                switch (parameters.Resolution)
                                {
                                    case SeriesResolution.Month:
                                        serieValue[0].Begin = new DateTime(serieValue[0].Begin.Year, serieValue[0].Begin.Month, 1, 0, 0, 0);
                                        break;
                                    default:
                                        break;
                                }

                                if (countersSeriesValue.ContainsKey(counterId))
                                {
                                    var existingSerie = countersSeriesValue[counterId];
                                    FixCounterSerieData(serieValue, parameters.Resolution, existingSerie[existingSerie.Count - 1].End);
                                    // it can be that there are multiple mappings over more then one mini that are active at the same time
                                    if (serieValue[0].Begin <= existingSerie[existingSerie.Count - 1].Begin)
                                    {
                                        var currentExistingSerieIndex = existingSerie.IndexOf(existingSerie.First(s => s.Begin == serieValue[0].Begin));
                                        var totalRecords = serieValue.Count;
                                        for (var i = 0; i < totalRecords; i++)
                                        {
                                            if (serieValue[0].Value.HasValue)
                                                existingSerie[currentExistingSerieIndex].Value = (existingSerie[currentExistingSerieIndex].Value ?? 0) + serieValue[0].Value;

                                            serieValue.RemoveAt(0);
                                            if (serieValue.Count == 0)
                                                break;

                                            currentExistingSerieIndex++;
                                            if (existingSerie.Count == currentExistingSerieIndex)
                                            {
                                                ((List<SeriesValue>)existingSerie).AddRange(serieValue);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ((List<SeriesValue>)existingSerie).AddRange(serieValue);
                                    }

                                }
                                else
                                {
                                    FixCounterSerieData(serieValue, parameters.Resolution, fromNl);
                                    countersSeriesValue.Add(counterId, serieValue);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // we do not want any errors to show up because it will invalidate the whole view
                        Log.ErrorException("Error in GetSeriesAtCounterLevel", e);
                    }
                }
            }
            finally
            {
                foreach (var counter in mini.Counters.Where(counter => counter.StorageProvider != null))
                {
                    counter.StorageProvider.Dispose();
                }
            }

            if (Log.IsTraceEnabled())
                Log.Trace(JsonUtils.ObjectToJsonString(countersSeriesValue));
            return countersSeriesValue;
        }

        private static Mini CreateMini(string qboxSerial)
        {
            // SAM: previously the Qbox metadata was read from Redis. For now we take a huge shortcut and
            // only support Qbox Duo with smart meter EG with S0.
            // This code is tied to a similar construct in Qserver (QboxDataDumpContextFactory.Mini).
            var qbox = new Qbox
            {
                SerialNumber = qboxSerial,
                Precision = Precision.mWh,
                DataStore = new DataStore
                {
                    Path = QboxNext.Core.Config.DataStorePath
                }
            };
            var mini = new Mini
            {
                Counters = new List<Counter>()
                {
                    new Counter
                    {
                        CounterId = 181,
                        GroupId = CounterSource.Client0,
                        CounterDeviceMappings = new List<CounterDeviceMapping>
                        {
                            new CounterDeviceMapping
                            {
                                Device = new Device
                                {
                                    EnergyType = DeviceEnergyType.Consumption
                                },
                                PeriodeBegin = new DateTime(2012, 1, 1),
                                PeriodeEind = new DateTime(9999, 1, 1)
                            }
                        },
                        Qbox = qbox
                    },
                    new Counter
                    {
                        CounterId = 182,
                        GroupId = CounterSource.Client0,
                        CounterDeviceMappings = new List<CounterDeviceMapping>
                        {
                            new CounterDeviceMapping
                            {
                                Device = new Device
                                {
                                    EnergyType = DeviceEnergyType.Consumption
                                },
                                PeriodeBegin = new DateTime(2012, 1, 1),
                                PeriodeEind = new DateTime(9999, 1, 1)
                            }
                        },
                        Qbox = qbox
                    },
                    new Counter
                    {
                        CounterId = 281,
                        GroupId = CounterSource.Client0,
                        CounterDeviceMappings = new List<CounterDeviceMapping>
                        {
                            new CounterDeviceMapping
                            {
                                Device = new Device
                                {
                                    EnergyType = DeviceEnergyType.Generation
                                },
                                PeriodeBegin = new DateTime(2012, 1, 1),
                                PeriodeEind = new DateTime(9999, 1, 1)
                            }
                        },
                        Qbox = qbox
                    },
                    new Counter
                    {
                        CounterId = 282,
                        GroupId = CounterSource.Client0,
                        CounterDeviceMappings = new List<CounterDeviceMapping>
                        {
                            new CounterDeviceMapping
                            {
                                Device = new Device
                                {
                                    EnergyType = DeviceEnergyType.Generation
                                },
                                PeriodeBegin = new DateTime(2012, 1, 1),
                                PeriodeEind = new DateTime(9999, 1, 1)
                            }
                        },
                        Qbox = qbox
                    },
                    new Counter
                    {
                        CounterId = 2421,
                        GroupId = CounterSource.Client0,
                        CounterDeviceMappings = new List<CounterDeviceMapping>
                        {
                            new CounterDeviceMapping
                            {
                                Device = new Device
                                {
                                    EnergyType = DeviceEnergyType.Gas
                                },
                                PeriodeBegin = new DateTime(2012, 1, 1),
                                PeriodeEind = new DateTime(9999, 1, 1)
                            }
                        },
                        Qbox = qbox
                    },
                    new Counter
                    {
                        CounterId = 1,
                        GroupId = CounterSource.Client0,
                        Secondary = true,
                        CounterDeviceMappings = new List<CounterDeviceMapping>
                        {
                            new CounterDeviceMapping
                            {
                                Device = new Device
                                {
                                    EnergyType = DeviceEnergyType.Generation
                                },
                                PeriodeBegin = new DateTime(2012, 1, 1),
                                PeriodeEind = new DateTime(9999, 1, 1)
                            }
                        },
                        Qbox = qbox
                    }
                }
            };

            foreach (var counter in mini.Counters)
            {
                counter.ComposeStorageid();
            }

            return mini;
        }

        private static void FixCounterSerieData(IList<SeriesValue> inSeriesValue, SeriesResolution inResolution, DateTime inFromDate)
        {
            // it can be the case that there is not data yet at the actualFrom date, this results in null values which should be 0
            if (inSeriesValue.Any(sv => !sv.Value.HasValue))
            {
                for (var i = 0; i < inSeriesValue.Count(v => v.End <= DateTime.UtcNow); i++)
                {
                    if (inSeriesValue[i].Value.HasValue)
                    {
                        break;
                    }
                    inSeriesValue[i].Value = 0;
                }
            }

            while (inSeriesValue[0].Begin > inFromDate)
            {
                var value = new SeriesValue();
                if (inResolution == SeriesResolution.Month)
                {
                    value.Begin = inSeriesValue[0].Begin.AddMonths(-1);
                    value.End = value.Begin.AddMonths(1);
                }
                else
                {
                    value.Begin = inSeriesValue[0].Begin.AddMinutes(-1 * (int)inResolution);
                    value.End = value.Begin.AddMinutes((int)inResolution);
                }


                value.Value = 0;
                inSeriesValue.Insert(0, value);
            }
        }
    }
}
