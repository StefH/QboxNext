using System;

namespace QboxNext.Simulation
{
	/// <summary>
	/// Counter implements a single counter consisting of an internal ID and a value, updated every minute.
	/// </summary>
	public class Counter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Counter(int inId)
		{
			Id = inId;

			UsagePattern = new UsagePatternSpec();
		}


		public int Id { get; private set; }
		public long Value { get; set; }
		public UsagePatternSpec UsagePattern { get; set; }

		private CounterValueGenerator Generator { get; set; }


		/// <summary>
		/// Update the counter with inTimestamp.
		/// </summary>
		public virtual void Update(DateTime inTimestamp)
		{
			Value = (long)GetGenerator().GetNext(inTimestamp);
		}


		/// <summary>
		/// Get the cached value generator.
		/// </summary>
		protected CounterValueGenerator GetGenerator()
		{
			if (Generator == null)
				Generator = CounterValueGenerator.CreateGenerator(UsagePattern);

			return Generator;
		}
	}


	/// <summary>
	/// Counter that is updated only hourly.
	/// </summary>
	public class HourlyCounter : Counter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="inId">Counter ID</param>
		public HourlyCounter(int inId) : base(inId)
		{
		}


		/// <summary>
		/// Update the counter with inTimestamp. Measure once every hour.
		/// </summary>
		public override void Update(DateTime inTimestamp)
		{
			var measurementTimestamp = new DateTime(inTimestamp.Year, inTimestamp.Month, inTimestamp.Day, inTimestamp.Hour, 0, 0);
			Value = (int)GetGenerator().GetNext(measurementTimestamp);
		}
	}
}
