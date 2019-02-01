
using System;

namespace QboxNext.Simulation
{
	/// <summary>
	/// Shape of the energy usage.
	/// </summary>
	public enum EUsageShape
	{
		None,
		Zero,
		ZeroPeak,
		Flat,
		FlatPeak,
		Block,
		BlockHalf,
		Sine,
		Random
	}


	/// <summary>
	/// Usage pattern specifier.
	/// </summary>
	public class UsagePatternSpec
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public UsagePatternSpec()
		{
			CounterId = DefaultCounterId;
			Shape = DefaultShape;
			Scale = DefaultScale;
			Period = DefaultPeriod;
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		public UsagePatternSpec(int inCounterId, EUsageShape inShape, float inScale, uint inPeriod, uint inSequenceOffset, uint inCounterOffset)
		{
			CounterId = inCounterId;
			Shape = inShape;
			Scale = inScale;
			Period = inPeriod;
			SequenceOffset = inSequenceOffset;
			CounterOffset = inCounterOffset;
		}


		public int CounterId { get; set; }
		public EUsageShape Shape { get; set; }
		public float Scale { get; set; }
		public uint SequenceOffset { get; set; }
		public uint CounterOffset { get; set; }


		/// <summary>
		/// Period of the counter function in number of measurements.
		/// </summary>
		public uint Period { get; set; }


		/// <summary>
		/// Start time of the period. null means the moment the first message is sent.
		/// </summary>
		public DateTime? PeriodStart { get; set; }


		public const int DefaultCounterId = 1;
		public const EUsageShape DefaultShape = EUsageShape.Block;
		public const float DefaultScale = 1.0f;
		public const int DefaultPeriod = 30;					// Period in number of measurements (ie. minutes).
	}
}
