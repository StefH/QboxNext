using System;
using System.Diagnostics;


namespace QboxNext.Core.Simulation
{
	/// <summary>
	/// Base class for counter value generators.
	/// </summary>
	public abstract class CounterValueGenerator
	{
		/// <summary>
		/// Default constructor, uses default usage pattern spec.
		/// </summary>
		protected CounterValueGenerator()
		{
			PatternSpec = new UsagePatternSpec();
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		protected CounterValueGenerator(UsagePatternSpec inSpec)
		{
			PatternSpec = inSpec;
		}


		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public abstract ulong GetNext(DateTime inTimestamp);


		/// <summary>
		/// Create a generator given a spec.
		/// </summary>
		public static CounterValueGenerator CreateGenerator(UsagePatternSpec inSpec)
		{
			switch (inSpec.Shape)
			{
				case EUsageShape.None:
					return null;
				case EUsageShape.Zero:
					inSpec.Scale = 0;
					return new ConstantUsageCounterValueGenerator(inSpec, false);
				case EUsageShape.ZeroPeak:
					inSpec.Scale = 0;
					return new ConstantUsageCounterValueGenerator(inSpec, true);
				case EUsageShape.Flat:
					return new ConstantUsageCounterValueGenerator(inSpec, false);
				case EUsageShape.FlatPeak:
					return new ConstantUsageCounterValueGenerator(inSpec, true);
				case EUsageShape.Random:
					return new RandomCounterValueGenerator(inSpec);
				case EUsageShape.Block:
					return new BlockUsageCounterValueGenerator(inSpec, StandardTicksIncrease * 2, StandardTicksIncrease);
				case EUsageShape.BlockHalf:
					return new BlockUsageCounterValueGenerator(inSpec, StandardTicksIncrease, 0);
				case EUsageShape.Sine:
					return new SineUsageCounterValueGenerator(inSpec);
				default:
					throw new NotImplementedException("Unknown shape " + inSpec.Shape);
			}
		}


		protected DateTime GetStartEpoch(DateTime inTimestamp)
		{
			if (_startEpoch == DateTime.MinValue)
			{
				_startEpoch = PatternSpec.PeriodStart ?? inTimestamp;
			}

			return _startEpoch;
		}


		public const uint StandardTicksIncrease = 10; 
		protected UsagePatternSpec PatternSpec;
		private DateTime _startEpoch = DateTime.MinValue;
	}


	/// <summary>
	/// Provides a counter that simulates constant usage.
	/// </summary>
	public class ConstantUsageCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ConstantUsageCounterValueGenerator(UsagePatternSpec inSpec, bool inHasPeak)
			: base(inSpec)
		{
			HasPeak = inHasPeak;
		}

		public bool HasPeak { get; set; }
		public DateTime StartTime { get; set; }

		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			if (StartTime == new DateTime())
				StartTime = inTimestamp;
			
			var elapsed = inTimestamp - GetStartEpoch(inTimestamp);
			var elapsedMinutes = (ulong)(elapsed.TotalMinutes + 0.5);

			double minutesToPeak = (StartTime.AddMinutes(PeakMomentInMinutes) - inTimestamp).TotalMinutes;
			if (HasPeak && minutesToPeak >= 0 && minutesToPeak < 1)
			{
				return (ulong) (elapsedMinutes * PatternSpec.Scale * StandardTicksIncrease + MaxPeakValue + PatternSpec.CounterOffset);
			}

			return (ulong)(elapsedMinutes * PatternSpec.Scale * StandardTicksIncrease + PatternSpec.CounterOffset);
		}
		
		public const int PeakMomentInMinutes = 2;
		public const int MaxPeakValue = 17250000;
	}


	/// <summary>
	/// Increase the counter with random amounts.
	/// </summary>
	class RandomCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RandomCounterValueGenerator(UsagePatternSpec inSpec)
			: base(inSpec)
		{
		}


		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			// To prevent having to iterate over all values from the start of the epoch, we use an average as the starting point for this value.
			var elapsed = inTimestamp - GetStartEpoch(inTimestamp);
			var elapsedMinutes = (ulong)elapsed.TotalMinutes;
			var baseTicks = (ulong)(elapsedMinutes * PatternSpec.Scale * StandardTicksIncrease);
			var extraTicks = (ulong)(mRandom.NextDouble() * PatternSpec.Scale * StandardTicksIncrease);
			return baseTicks + extraTicks + PatternSpec.CounterOffset;
		}


		private Random mRandom = new Random();
	}


	/// <summary>
	/// Generate a block usage pattern.
	/// </summary>
	class BlockUsageCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public BlockUsageCounterValueGenerator(UsagePatternSpec inSpec, uint inHighIncrement, uint inLowIncrement)
			: base(inSpec)
		{
			mHighIncrement = inHighIncrement;
			mLowIncrement = inLowIncrement;
		}

		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			var elapsed = inTimestamp - GetStartEpoch(inTimestamp);
			uint elapsedMinutes = (uint)elapsed.TotalMinutes;
			uint elapsedCycles = (uint)(elapsedMinutes / PatternSpec.Period);

			ulong counterValue = elapsedCycles * ((mHighIncrement + mLowIncrement) / 2) * PatternSpec.Period;

			uint offsetInCycle = elapsedMinutes - elapsedCycles * PatternSpec.Period;
			for (int i = 0; i < offsetInCycle; ++i)
			{
				if (i + PatternSpec.SequenceOffset < PatternSpec.Period / 2 || i + PatternSpec.SequenceOffset >= PatternSpec.Period)
					counterValue += mHighIncrement;
				else
					counterValue += mLowIncrement;
			}

			return counterValue + PatternSpec.CounterOffset;
		}


		private readonly uint mHighIncrement;
		private readonly uint mLowIncrement;
	}


	/// <summary>
	/// Generate a block usage pattern.
	/// </summary>
	class SineUsageCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public SineUsageCounterValueGenerator(UsagePatternSpec inSpec)
			: base(inSpec)
		{
		}


		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			var elapsed = inTimestamp - GetStartEpoch(inTimestamp);
			// To prevent the first value from getting below zero and generating an incorrect value, we skip the first minute.
			double t = (uint)elapsed.TotalMinutes + 1;

			const double highIncrement = StandardTicksIncrease * 2;
			const double lowIncrement = StandardTicksIncrease;
			const double m = (highIncrement + lowIncrement) / 2.0;

			double a = highIncrement - m;
			double b = PatternSpec.SequenceOffset;
			double f = 2 * Math.PI / PatternSpec.Period;

			// Formula for the desired graph in day graph: m + a * sin(f * t + b)
			// Integrated formula:
			double val = Math.Abs(Math.Round(m * t - (a / f) * Math.Cos(f * t + b)));
			
			Debug.Assert(val >= 0.0);
			var counterValue = (ulong)val;
			return counterValue + PatternSpec.CounterOffset;
		}
	}


	/// <summary>
	/// Generator that has increasing usage over the months.
	/// </summary>
	class IncreasingOverMonthCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public IncreasingOverMonthCounterValueGenerator(UsagePatternSpec inSpec)
			: base(inSpec)
		{
		}


		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			//TODO
			throw new NotImplementedException();
		}
	}


	/// <summary>
	/// Generator that has decreasing usage over the months.
	/// </summary>
	class DecreasingOverMonthCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			//TODO
			throw new NotImplementedException();
		}
	}


	/// <summary>
	/// Generator that has increasing usage over the days.
	/// </summary>
	class IncreasingOverDaysCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			//TODO
			throw new NotImplementedException();
		}
	}


	/// <summary>
	/// Generator that has decreasing usage over the days.
	/// </summary>
	class DecreasingOverDaysCounterValueGenerator : CounterValueGenerator
	{
		/// <summary>
		/// Get next counter value given a timestamp.
		/// </summary>
		public override ulong GetNext(DateTime inTimestamp)
		{
			//TODO
			throw new NotImplementedException();
		}
	}
}
