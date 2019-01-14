using System;
using System.Collections.Generic;
using System.Diagnostics;
using QboxNext.Core.Simulation;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Utils;
using QboxNext.Qserver.Core.Extensions;

namespace QboxNext.Qserver.Core.Statistics
{
	/// <summary>
	/// One slot in a series.
	/// </summary>
	[DebuggerDisplay("{Begin}-{End}:{Value}")]
	public class SeriesValue
	{
		/// <summary>
		/// Default constructor, needed for serialization.
		/// </summary>
		public SeriesValue()
		{
		}


		/// <summary>
		/// Constructor, leaves Value at null.
		/// </summary>
		public SeriesValue(DateTime inBegin, DateTime inEnd)
		{
			Begin = inBegin;
			End = inEnd;
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		public SeriesValue(DateTime inBegin, DateTime inEnd, decimal inValue)
		{
			Begin = inBegin;
			End = inEnd;
			Value = inValue;
		}

		public decimal? Value { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }

		protected bool Equals(SeriesValue other)
		{
			return Value == other.Value && Begin.Equals(other.Begin) && End.Equals(other.End);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((SeriesValue)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Value.GetHashCode();
				hashCode = (hashCode * 397) ^ Begin.GetHashCode();
				hashCode = (hashCode * 397) ^ End.GetHashCode();
				return hashCode;
			}
		}
	}

	public class CounterSerie
	{
		public string Name { get; set; }
		public IList<SeriesValue> SeriesData { get; set; }
	}

	public class CounterSeries
	{
		public CounterSeries()
		{
			Series = new List<CounterSerie>();
		}

		public IList<CounterSerie> Series { get; set; }
	}

	public static class SeriesValueListBuilder
	{
		/// <summary>
		/// Builds the series without values.
		/// </summary>
		/// <param name="to">To.</param>
		/// <param name="from">From.</param>
		/// <param name="resolution">The resolution.</param>
		public static List<SeriesValue> BuildSeries(DateTime from, DateTime to, SeriesResolution resolution)
		{
			var serie = new List<SeriesValue>();

			// naar beneden afronden op hele minuten
			@from = @from.Truncate(TimeSpan.FromMinutes(1));
			to = @to.Truncate(TimeSpan.FromMinutes(1));

			DateTime calculatedFrom;
			DateTime calculatedTo;

			switch (resolution)
			{
				case SeriesResolution.OneMinute:
					calculatedFrom = @from;
					calculatedTo = to;
					break;
				case SeriesResolution.FiveMinutes:
					calculatedFrom = @from.AddMinutes((@from.Minute % 5) * -1);
					calculatedTo = to.AddMinutes((to.Minute % 5 > 0 ? 5 - to.Minute % 5 : 0));
					break;
				case SeriesResolution.Hour:
					calculatedFrom = @from.AddMinutes((@from.Minute % 60) * -1);
					calculatedTo = to.AddMinutes((to.Minute % 60 > 0 ? 60 - to.Minute % 60 : 0));
					break;
				case SeriesResolution.Day:
					calculatedFrom = @from.Date;
					calculatedTo = to.TimeOfDay.TotalMinutes > 0 ? to.AddDays(1).Date : to;
					break;
				case SeriesResolution.Week:
					var daysToWeekBegin = (int)@from.DayOfWeek;
					var daysToWeekEnd = 6 - (int)to.DayOfWeek;
					calculatedFrom = @from.Date.AddDays(daysToWeekBegin * -1d);
					calculatedTo = to.Date.AddDays(daysToWeekEnd + 1).Date;
					break;
				case SeriesResolution.Month:
					var refDate = @from.Date;
					while (refDate < to)
					{
						var endDate = refDate.AddMonths(1).FirstDayOfMonth();
						serie.Add(new SeriesValue(refDate, endDate < to ? endDate : to));
						refDate = endDate;
					}
					return serie;
				default:
					throw new ArgumentOutOfRangeException("resolution");
			}
			var span = calculatedTo - calculatedFrom;
			var nrOfValues = (int)span.TotalMinutes / (int)resolution;

			for (var i = 0; i < nrOfValues; i++)
			{
				var time = calculatedFrom.AddMinutes(i * (int)resolution);
				serie.Add(new SeriesValue(time, time.AddMinutes((int)resolution)));
			}
			return serie;
		}

		/// <summary>
		/// Genereren van waarden tbv een serie.
		/// Per 5 minuten wordt een waarde berekend mbv 'sin' functie.
		/// Over de hele dag is 1 volledige sinus beschikbaar
		/// </summary>
		/// <param name="values"> </param>
		/// <param name="factor">Bepaald de hoogte van de sinus, negatieve waarde gebruiken voor opwekking</param>
		/// <param name="cycles"> </param>
		/// <param name="yOffset"> </param>
		/// <param name="negate"> </param>
		/// <returns>List met iedere 5 minuten een waarde van _from tot _to</returns>
		public static void BuildData(IList<SeriesValue> values, double factor, double cycles, int yOffset, bool negate)
		{
			var random = new Random();

			var i = 0;
			foreach (var value in values)
			{
				var xOffset = value.Begin >= DateTime.Today ? 0 : random.Next(45);
				//if (i % 1440 == 0)
				//{
				//    if (_from.AddMinutes(i) >= DateTime.Today)
				//    {
				//        xOffset = 0;  // Geen offset bij huidige dag
				//    }
				//    else
				//    {
				//        xOffset = random.Next(45); // random offset bij vorige dagen
				//    }
				//}

				if (value.Begin < DateTime.Now)
				{
					value.Value = (decimal?)((Math.Sin((i + xOffset) / 1440.0 * 360.0 * Math.PI / 180.0 * cycles) + 1.0) * factor);
					if (negate)
					{
						value.Value = value.Value * -1;
					}
				}
				i++;
			}
		}


		/// <summary>
		/// Generate values using a value generator.
		/// </summary>
		public static void BuildData(IList<SeriesValue> values, CounterValueGenerator generator, bool isElectricity)
		{
			foreach (var value in values)
			{
				if (value.Begin < DateTime.Now)
				{
					var startTicks = generator.GetNext(value.Begin);
					var endTicks = generator.GetNext(value.End);
					double diffTicks = endTicks - startTicks;
					double usage = 0;

					// For electricity, assume 1000 ticks per kWh.
					// For gas, assume 1000 ticks per m3.
					if (isElectricity)
						// This is the amount of counter ticks that have elapsed. Now we have to compute the average usage.
						// For example 10 ticks per minute = 50 ticks per minute
						// Assume 1000 ticks per kWh, so 50 ticks equals 50 Wh equals 50 * 3600 Ws.
						// Dividing this by the elapsed seconds will give the average usage.
						usage = diffTicks * 3600.0 / (value.End - value.Begin).TotalSeconds;
					else
						usage = diffTicks / 1000.0;

					value.Value = (decimal)usage;
				}
			}
		}
	}
}
