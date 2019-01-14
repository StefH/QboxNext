using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QboxNext.Core.Simulation
{
	public enum QboxState
    {    
        Waiting = 0,
        Operational = 1,
        ValidImage = 4,
        InvalidImage = 5,
        HardReset = 6,
        UnexpectedReset = 7,
    }


	public enum QboxTimeState
	{
		Unreliable = 0,
		Reliable = 1
	}


	public class QboxMessageBuilder
	{
		public QboxMessageBuilder(Meter inMeter, bool inIsDuo, bool inSendClientStatus)
		{
			Meter = inMeter;
			QboxIsDuo = inIsDuo;
			SendClientStatus = inSendClientStatus;
		}


		/// <summary>
		/// Build the message string from one or more counters.
		/// </summary>
		public string BuildMessageString(int inProtocolVersion, int inSequenceNr, DateTime inTimestamp)
		{
			var sb = new StringBuilder();
			sb.Append((char)0x02);							// STX, start transmission
			sb.Append(inProtocolVersion.ToString("X2"));	// Protocol, from version 7 equal to firmware version
			var seqnr = (byte)(inSequenceNr & 0xFF);
			sb.Append(seqnr.ToString("X2"));				// Sequence nr
			var status = (byte)(((byte)(_qboxState) << 2) + ((byte)(QboxTimeState.Reliable) << 1) + 1);	// Status, Operational + time reliable + valid response
			sb.Append(status.ToString("X2"));
			int timestamp = Convert.ToInt32((inTimestamp.Subtract(new DateTime(2007, 1, 1))).TotalSeconds);
			sb.Append(timestamp.ToString("X8"));			// Timestamp
			byte meterType = Meter.GetMeterTypeForMessage(QboxIsDuo);

			sb.Append(meterType.ToString("X2"));			// other Meter type
			
			sb.Append(Meter.GetCountersPayload(inTimestamp, inProtocolVersion, seqnr, QboxIsDuo, SendClientStatus));
			sb.Append((char)0x03);							// ETX, end transmission

			return sb.ToString();
		}


		private QboxState _qboxState = QboxState.Operational;
		public QboxState QboxState
		{
			get { return _qboxState; }
			set { _qboxState = value; }
		}


		public Meter Meter { get; set; }


		public bool	QboxIsDuo { get; set; }


		public bool SendClientStatus { get; set; }
	}
}
