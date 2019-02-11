using System;
using System.Diagnostics;

namespace QboxNext.Storage
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
}
