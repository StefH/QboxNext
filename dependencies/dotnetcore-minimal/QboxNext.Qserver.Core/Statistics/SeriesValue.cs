using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	}
}
