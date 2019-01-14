using System;
using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	/// <summary>
    /// MiniR16, Firmware 39 (0x27)
    /// - ClientID added to client settings and client info DeviceSettings
    /// - GroupID added to counterpayloads
    /// </summary>
	public class MiniR16 : MiniR07
	{
		private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

		protected virtual void AddCounterPayload(Mini07ParseModel model, CounterGroup group)
		{
			model.Payloads.Add(new CounterWithSourcePayload
			{
				InternalNr = Parser.ParseByte(),
				Value = Parser.ParseUInt32(),
				PrimaryMeter = group.PrimaryMeterCounters,
				Source = group.CounterSource
			});
		}

        protected override void ParseCounters(Mini07ParseModel model)
        {
            Log.DebugFormat("Nr of countergroups: {nrOfCounters}", model.PayloadIndicator.NrOfCounters);
            int i = 0;
            // Total number of counters
            while (i < model.PayloadIndicator.NrOfCounters)
            {
                try
                {
                    // Grouped per CounterGroup
                    var group = new CounterGroup(Parser.ParseByte());
                    for (var c = 0; c < group.NbrOfCounters; c++)
                    {
						AddCounterPayload(model, group);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Parsing counter payload (payload {0} of {1})", i + 1, model.PayloadIndicator.NrOfCounters), ex);
                }
            }
        }
    }
}
