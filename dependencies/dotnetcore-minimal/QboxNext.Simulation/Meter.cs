using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace QboxNext.Simulation
{
	public enum MeterType
	{
		Ferraris,
		FerrarisS0,
		Led,
		LedS0,
		Smart,
		Soladin,
		SmartS0,
		NoMeter				// Z-wave host (not connected to meter)
	}


	public class MeterTypeName
	{
		public const string Led = "led";
		public const string LedS0 = "leds0";
		public const string Smart = "smart";
		public const string Ferraris = "ferraris";
		public const string FerrarisS0 = "ferrariss0";
		public const string Soladin = "soladin";
		public const string SmartS0 = "smarts0";
		public const string Generic = "generic";
	}


	public abstract class Meter
	{
		public abstract MeterType MeterType { get;  }


		public static MeterType GetMeterType(string inMeterTypeName)
		{
			return MeterTypeMap[inMeterTypeName];
		}


		/// <summary>
		/// Create a specific meter.
		/// </summary>
		public static Meter CreateMeter(string inMeterTypeName)
		{
			return CreateMeter(GetMeterType(inMeterTypeName));
		}


		/// <summary>
		/// Create a specific meter.
		/// </summary>
		public static Meter CreateMeter(MeterType inMeterType)
		{
			switch (inMeterType)
			{
				case MeterType.Led:
					return new LedMeter();
				case MeterType.LedS0:
					return new LedS0Meter();
				case MeterType.Ferraris:
					return new FerrarisMeter();
				case MeterType.FerrarisS0:
					return new FerrarisS0Meter();
				case MeterType.Smart:
					return new SmartMeter();
				case MeterType.Soladin:
					return new Soladin();
				case MeterType.SmartS0:
					return new SmartS0Meter();
				case MeterType.NoMeter:
					return new NoMeter();
				default:
					throw new NotImplementedException("Unknown meter type " + inMeterType);
			}
		}


		public abstract byte GetMeterTypeForMessage(bool inIsDuo);
		// LED_TypeI = 0,				(counter 1)
		// LED_TypeII = 1,
		// Ferraris_Black_Toothed = 2,	(counter 1 en 3)
		// Smart_Meter_E = 6,
		// Smart_Meter_EG = 7,
		// SO_Pulse = 8,
		// Soladin_600 = 9,
		// No_Meter = 30


		public virtual void SetUsagePatterns(List<UsagePatternSpec> inSpecs)
		{
			foreach (var spec in inSpecs)
			{
				var counter = CreateCounter(spec.CounterId);
				counter.UsagePattern = spec;
				Counters.Add(counter);
			}
		}


		/// <summary>
		/// Create a proper counter object for the given counter ID.
		/// </summary>
		private Counter CreateCounter(int inCounterId)
		{
			if (inCounterId != CounterIdSmartGas)
				return new Counter(inCounterId);
			else
				return new HourlyCounter(inCounterId);
		}


		public List<UsagePatternSpec> GetUsagePatterns()
		{
			return Counters.Select(counter => counter.UsagePattern).ToList();
		}


		protected void UpdateCounters(DateTime inTimestamp)
		{
			foreach (var counter in Counters)
				counter.Update(inTimestamp);
		}


		public abstract string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus);


		public abstract int ActivationMeterId { get; }


		/// <summary>
		/// Returns a nice Name that can be used for Interfaces
		/// </summary>
		public string Name
		{
			get
			{
				string name = GetType().Name;

				if (name.EndsWith("Meter"))
					return name.Substring(0, name.Length - 5);
				else
					return name;
			}
		}

		/// <summary>
		/// Simple implementation of GetCountersPayload. Iterates over counters and creates a simple counters payload.
		/// </summary>
		protected string GetSimpleCountersPayload(DateTime inTimestamp)
		{
			UpdateCounters(inTimestamp);

			var sb = new StringBuilder();
			var nrCounters = (byte)Counters.Count;
			sb.Append(nrCounters.ToString("X2"));			// Payload indicator, 1 counter
			foreach (var counter in Counters)
			{
				sb.Append(counter.Id.ToString("X2"));		// Counter ID
				sb.Append(counter.Value.ToString("X8"));	// Counter value
			}

			return sb.ToString();
		}

		protected string GetClientStatusResponse(int inProtcolVersion, int inNbrOfEntries)
		{
			// ClientId is added in protocol version 41
			var clientId = inProtcolVersion >= 41 ? "00" : "";
			var clientState = 1 + 4; // Connection with client (1) & Operational (4)
			return String.Format("{0}{1}{2}", inNbrOfEntries.ToString("X2"), clientId, clientState.ToString("X2"));
		}

		protected string GetProductAndSerialNumberResponse()
		{
			var sb = new StringBuilder();
			sb.Append("4000"); // 40: Request PN/SN, 
			sb.Append("\"6618-1200-2306/13-09-002-296\""); // string 128 with PN/SN.  Copied from PROD 13-09-002-296.

			return sb.ToString();
		}

		protected string GetFirwareVersionResponse()
		{
			return "430028";	// Client firmware response. 43: request client firmware version, 00: Client 1 and 28: firmware version.  Copied from PROD 13-09-002-296.
		}

		protected void GetCommandsAllowed(int inSequenceNumber, bool inIsDuo, bool inSendClientStatus, out bool ioIsPrimaryCommandAllowed, out bool ioIsSecondaryCommandAllowed, out bool ioIsTertiaryCommandAllowed, out bool ioIsClientStatusAllowed)
		{
			// Client Status allowed
			ioIsClientStatusAllowed = inSendClientStatus && inSequenceNumber % 7 == 0;
			// Actually if Device Settings are allowed
			ioIsPrimaryCommandAllowed = inSendClientStatus && inSequenceNumber % 3 == 0;
			ioIsSecondaryCommandAllowed = inSendClientStatus && inIsDuo && inSequenceNumber % 3 == 1;
			ioIsTertiaryCommandAllowed = inSendClientStatus && inIsDuo && inSequenceNumber % 3 == 2;
		}

		protected string GetPayloadIndicator(int inNbrOfCounters, bool inClientStatus, bool inDeviceSettings)
		{
			var val = inNbrOfCounters + (inClientStatus ? ClientStatusFlag : 0) + (inDeviceSettings ? DeviceSettingsFlag : 0);
			return val.ToString("X2");
		}


		public const int CounterIdFerrarisElectricityReceived = 1;
		public const int CounterIdFerrarisElectricityDelivered = 3;
		public const int CounterIdLedElectricityReceived = 1;
		public const int CounterIdSmartElectricityReceivedTariff1 = 181;
		public const int CounterIdSmartElectricityReceivedTariff2 = 182;
		public const int CounterIdSmartElectricityDeliveredTariff1 = 281;
		public const int CounterIdSmartElectricityDeliveredTariff2 = 282;
		public const int CounterIdSmartGas = 2421;
		public const int CounterIdSoladin = 120;

		protected const int ClientStatusFlag = 16;
		protected const int DeviceSettingsFlag = 64;
		protected readonly List<Counter> Counters = new List<Counter>();

		private static readonly Dictionary<string, MeterType> MeterTypeMap = new Dictionary<string, MeterType>()
		{
			{ MeterTypeName.Ferraris, MeterType.Ferraris },
			{ MeterTypeName.FerrarisS0, MeterType.FerrarisS0 },
			{ MeterTypeName.Led, MeterType.Led },
			{ MeterTypeName.LedS0, MeterType.LedS0 },
			{ MeterTypeName.Smart, MeterType.Smart },
			{ MeterTypeName.Soladin, MeterType.Soladin },
			{ MeterTypeName.SmartS0, MeterType.SmartS0 },
			{ MeterTypeName.Generic, MeterType.NoMeter }
		};
	}


	public class FerrarisMeter : Meter
	{
		public override MeterType MeterType { get { return MeterType.Ferraris;  } }


		public override byte GetMeterTypeForMessage(bool inIsDuo)
		{
			if (inIsDuo)
				return 30;  // No_Meter

			return 2;		// Ferraris_Black_Toothed
		}


		public override int ActivationMeterId
		{
			get { return 0; }
		}


		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			UpdateCounters(inTimestamp);

			var sb = new StringBuilder();
			var nrCounters = Counters.Count;

			// If we can send a primary command on sequence nr X, we can send the secondary command on sequence nr X+1, etc.
			bool isPrimaryCommandAllowed;
			bool isSecondaryCommandAllowed;
			bool isTertiaryCommandAllowed;
			bool isClientStatusAllowed;
			GetCommandsAllowed(inSequenceNumber, inIsDuo, inSendClientStatus, out isPrimaryCommandAllowed, out isSecondaryCommandAllowed, out isTertiaryCommandAllowed, out isClientStatusAllowed);

			sb.Append(GetPayloadIndicator(nrCounters, isClientStatusAllowed, isPrimaryCommandAllowed || isSecondaryCommandAllowed || isTertiaryCommandAllowed));	// Payload indicator, 2 counters and DeviceSettingPresent

			if (isClientStatusAllowed)
				sb.Append(GetClientStatusResponse(inProtocolVersion, 1));

			if (isPrimaryCommandAllowed)
				sb.Append(RequestSensorValues);
			else if (isSecondaryCommandAllowed)
				sb.Append(GetProductAndSerialNumberResponse());
			else if (isTertiaryCommandAllowed)
				sb.Append(GetFirwareVersionResponse());
			
			sb.Append((240 + nrCounters).ToString("X2"));	// Group ID, Primary meter, source - Host / Mono (Basic) with 2 counters.

			foreach (var counter in Counters)
			{
				sb.Append(counter.Id.ToString("X2"));		// Counter ID
				sb.Append(counter.Value.ToString("X8"));	// Counter value
			}

			return sb.ToString();
		}

		protected const string RequestSensorValues = "0A06080B070A7F08080B090A79";	// Composite ID: 0A, ID: 06 and VALUE: 0A63. <composite ID><ID><VALUE><ID><VALUE>
	}


	public class FerrarisS0Meter : Meter
	{
		public override MeterType MeterType { get { return MeterType.FerrarisS0; } }


		public override byte GetMeterTypeForMessage(bool inIsDuo)
		{
			if (inIsDuo)
				return 30;  // No_Meter

			return 2;		// Ferraris_Black_Toothed
		}


		public override int ActivationMeterId
		{
			get { return 0; }
		}


		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			UpdateCounters(inTimestamp);

			var sb = new StringBuilder();
			var nrCounters = Counters.Count;

			// If we can send a primary command on sequence nr X, we can send the secondary command on sequence nr X+1, etc.
			bool isPrimaryCommandAllowed;
			bool isSecondaryCommandAllowed;
			bool isTertiaryCommandAllowed;
			bool isClientStatusAllowed;
			GetCommandsAllowed(inSequenceNumber, inIsDuo, inSendClientStatus, out isPrimaryCommandAllowed, out isSecondaryCommandAllowed, out isTertiaryCommandAllowed, out isClientStatusAllowed);

			sb.Append(GetPayloadIndicator(nrCounters, isClientStatusAllowed, isPrimaryCommandAllowed || isSecondaryCommandAllowed || isTertiaryCommandAllowed));	// Payload indicator, 2 counters and DeviceSettingPresent

			if (isClientStatusAllowed)
				sb.Append(GetClientStatusResponse(inProtocolVersion, 1));

			if (isPrimaryCommandAllowed)
				sb.Append(RequestSensorValues);
			else if (isSecondaryCommandAllowed)
				sb.Append(GetProductAndSerialNumberResponse());
			else if (isTertiaryCommandAllowed)
				sb.Append(GetFirwareVersionResponse());

			if (!inIsDuo)
				sb.Append((0xF2).ToString("X2")); // Group ID, Primary meter, source - Host / Mono (Basic) with 2 counters.
			else
				sb.Append(0x82.ToString("X2")); // Group ID, Primary meter with 2 counters for the Ferraris meter.

			// Ferraris Counter ID 1
			var counter3 = Counters.FirstOrDefault(c => c.Id == 3);
			if (counter3 != null)
			{
				sb.Append(counter3.Id.ToString("X2")); // Counter ID
				sb.Append(counter3.Value.ToString("X8")); // Counter value
			}

			var counter1Ferraris = Counters.FirstOrDefault(c => c.Id == 1);
			if (counter1Ferraris != null)
			{
				sb.Append(counter1Ferraris.Id.ToString("X2")); // Counter ID
				sb.Append(counter1Ferraris.Value.ToString("X8")); // Counter value
			}

			// S0 meter
			var allCounter1s = Counters.Where(c => c.Id == 1).ToList();
			if (allCounter1s.Count() == 2)
			{
				var counter1S0 = allCounter1s.Skip(1).Take(1).ToList().First();

				if (!inIsDuo)
					sb.Append(0x71.ToString("X2")); // Group ID, Secondary meter, 1 counter for S0 meter.
				else
					sb.Append(0x01.ToString("X2")); // Group ID, Primary meter, 1 counter for S0 meter.

				sb.Append(counter1S0.Id.ToString("X2")); // Counter ID
				sb.Append(counter1S0.Value.ToString("X8")); // Counter value
			}

			return sb.ToString();
		}

		protected const string RequestSensorValues = "0A06080B070A7F08080B090A79";	// Composite ID: 0A, ID: 06 and VALUE: 0A63. <composite ID><ID><VALUE><ID><VALUE>
	}
	
	
	public class LedMeter : Meter
	{
		public override MeterType MeterType { get { return MeterType.Led; } }


		public override byte GetMeterTypeForMessage(bool inIsDuo)
		{
			if (inIsDuo)
				return 30;  // No_Meter

			return 0;		// LED_TypeI
		}


		public override int ActivationMeterId
		{
			get { return 32768; }
		}


		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			UpdateCounters(inTimestamp);

			var sb = new StringBuilder();
			var nrCounters = Counters.Count;
			
			// If we can send a primary command on sequence nr X, we can send the secondary command on sequence nr X+1, etc.
			bool isPrimaryCommandAllowed;
			bool isSecondaryCommandAllowed;
			bool isTertiaryCommandAllowed;
			bool isClientStatusAllowed;
			GetCommandsAllowed(inSequenceNumber, inIsDuo, inSendClientStatus, out isPrimaryCommandAllowed, out isSecondaryCommandAllowed, out isTertiaryCommandAllowed, out isClientStatusAllowed);

			sb.Append(GetPayloadIndicator(nrCounters, isClientStatusAllowed, isPrimaryCommandAllowed || isSecondaryCommandAllowed || isTertiaryCommandAllowed));	// Payload indicator, 1 counter and DeviceSettingPresent

			if (isClientStatusAllowed)
				sb.Append(GetClientStatusResponse(inProtocolVersion, 1));

			if (isPrimaryCommandAllowed)
				sb.Append(RequestLedChannelResponse);
			else if (isSecondaryCommandAllowed)
				sb.Append(GetProductAndSerialNumberResponse());
			else if (isTertiaryCommandAllowed)
				sb.Append(GetFirwareVersionResponse());

			if (inIsDuo)
				sb.Append((128 + nrCounters).ToString("X2")); // Group ID, Primary meter, client, 1 counter.
			else
				sb.Append((240 + nrCounters).ToString("X2")); // Group ID, Primary meter, host, 1 counter.

			foreach (var counter in Counters)
			{
				sb.Append(counter.Id.ToString("X2"));		// Counter ID
				sb.Append(counter.Value.ToString("X8"));	// Counter value
			}

			return sb.ToString();
		}

		protected const string RequestLedChannelResponse = "2003";	// LED sensor Channel device setting + Channel number 1
	}


	public class LedS0Meter : LedMeter
	{
		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			UpdateCounters(inTimestamp);

			var sb = new StringBuilder();
			var nrCounters = Counters.Count;

			// If we can send a primary command on sequence nr X, we can send the secondary command on sequence nr X+1, etc.
			bool isPrimaryCommandAllowed;
			bool isSecondaryCommandAllowed;
			bool isTertiaryCommandAllowed;
			bool isClientStatusAllowed;
			GetCommandsAllowed(inSequenceNumber, inIsDuo, inSendClientStatus, out isPrimaryCommandAllowed, out isSecondaryCommandAllowed, out isTertiaryCommandAllowed, out isClientStatusAllowed);

			// Note that Qbox-duo Led+S0 is behaving weird: it's sending two counters because it
			// still somehow thinks that the primary meter is a LED.
			// However, in the current version of the firmware this LED-counter is exactly the same
			// as the S0 counter. Not logical, but we replicate that behaviour here.
			sb.Append(GetPayloadIndicator(inIsDuo ? 2 : 1, isClientStatusAllowed, isPrimaryCommandAllowed || isSecondaryCommandAllowed || isTertiaryCommandAllowed));	// Payload indicator, 2 counters for duo and 1 counter for mono and DeviceSettingPresent

			if (isClientStatusAllowed)
				sb.Append(GetClientStatusResponse(inProtocolVersion, 1));

			if (isPrimaryCommandAllowed)
				sb.Append(RequestLedChannelResponse);
			else if (isSecondaryCommandAllowed)
				sb.Append(GetProductAndSerialNumberResponse());
			else if (isTertiaryCommandAllowed)
				sb.Append(GetFirwareVersionResponse());

			if (inIsDuo)
			{
				// Duplicate counter values for LED Counter 1, which is not used
				sb.Append((128 + nrCounters).ToString("X2"));	// Group ID, Primary meter, client, 1 counter.
				sb.Append(Counters[0].Id.ToString("X2"));		// Counter ID
				sb.Append(Counters[0].Value.ToString("X8"));	// Counter value

				// S0 Counter 1
				sb.Append((nrCounters).ToString("X2"));			// Group ID, Secondary Meter, client, 1 counter.
				sb.Append(Counters[0].Id.ToString("X2"));		// Counter ID
				sb.Append(Counters[0].Value.ToString("X8"));	// Counter value
			}
			else
			{
				sb.Append((112 + nrCounters).ToString("X2"));	// Group ID, Secondary Meter, host, 1 counter.
				sb.Append(Counters[0].Id.ToString("X2"));		// Counter ID
				sb.Append(Counters[0].Value.ToString("X8"));	// Counter value
			}

			return sb.ToString();
		}
	}


	public class SmartMeter : Meter
	{
		public override MeterType MeterType { get { return MeterType.Smart; } }


		public override byte GetMeterTypeForMessage(bool inIsDuo)
		{
			if (inIsDuo)
				return 30;  // No_Meter

			return 7;		// Smart_Meter_EG
		}


		public override int ActivationMeterId
		{
			get { return 49152; }
		}


		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			UpdateCounters(inTimestamp);
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			var sb = new StringBuilder();

			if (!inIsDuo)
			{
				sb.Append(0x80.ToString("X2")); // Payload indicator, smart meter present.		    
				// Payload copied from 12-13-001-057 on ACC.
				string cPayloadFormat =
					inProtocolVersion < 41 ?
@"/ISk5\2MT382-1003
0-0:96.1.1(5A424556303035303639323630373131)
1-0:1.8.1({0:00000.000}*kWh)
1-0:1.8.2({1:00000.000}*kWh)
1-0:2.8.1({2:00000.000}*kWh)
1-0:2.8.2({3:00000.000}*kWh)
0-0:96.14.0(0002)
1-0:1.7.0(0000.00*kW)
1-0:2.7.0(0000.00*kW)
0-0:17.0.0(0000.00*kW)
0-0:96.3.10(1)
0-0:96.13.1()
0-0:96.13.0()
0-1:24.1.0(3)
0-1:96.1.0(3238303131303038323036303536343132)
0-1:24.3.0(121012140000)(00)(60)(1)(0-1:24.2.1)(m3)({4:00000.000})
0-1:24.4.0(1)
!
" :
@"/KFM5KAIFA-METER 1-3:0.2.8(40) 0-0:1.0.0(000102045905W) 
0-0:96.1.1(4530303033303030303030303032343133) 
1-0:1.8.1({0:00000.000}*kWh) 
1-0:1.8.2({1:00000.000}*kWh) 
1-0:2.8.1({2:00000.000}*kWh) 
1-0:2.8.2({3:00000.000}*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(00.034*kW) 1-0:2.7.0(00.000*kW) 
0-0:17.0.0(999.9*kW) 0-0:96.3.10(1) 0-0:96.7.21(00073) 0-0:96.7.9(00020) 
1-0:99.97.0(3)(0-0:96.7.19)(000124235657W)(0000003149*s)(000124225935W)(0000000289*s)(000101000001W)(2147483647*s) 
1-0:32.32.0(00005) 1-0:52.32.0(00006) 1-0:72.32.0(00001) 1-0:32.36.0(00000) 
1-0:52.36.0(00000) 1-0:72.36.0(00000) 0-0:96.13.1() 0-0:96.13.0() 1-0:31.7.0(000*A) 
1-0:51.7.0(000*A) 1-0:71.7.0(000*A) 1-0:21.7.0(00.034*kW) 
1-0:22.7.0(00.000*kW) 1-0:41.7.0(00.000*kW) 1-0:42.7.0(00.000*kW) 
1-0:61.7.0(00.000*kW) 1-0:62.7.0(00.000*kW) 0-1:24.1.0(003) 
0-1:96.1.0(4730303131303033303832373133363133) 0-1:24.2.1(000102043601W)({4:00000.000}*m3) 
0-1:24.4.0(1) !583C";

				sb.AppendFormat(cPayloadFormat,
								Counters.First(c => c.Id == 181).Value / 1000.0d,
								Counters.First(c => c.Id == 182).Value / 1000.0d,
								Counters.First(c => c.Id == 281).Value / 1000.0d,
								Counters.First(c => c.Id == 282).Value / 1000.0d,
								Counters.First(c => c.Id == 2421).Value / 1000.0d);

				return sb.ToString();
			}

			// IsDuo
			bool isPrimaryCommandAllowed;
			bool isSecondaryCommandAllowed;
			bool isTertiaryCommandAllowed;
			bool isClientStatusAllowed;
			GetCommandsAllowed(inSequenceNumber, inIsDuo, inSendClientStatus, out isPrimaryCommandAllowed, out isSecondaryCommandAllowed, out isTertiaryCommandAllowed, out isClientStatusAllowed);

			sb.Append(GetPayloadIndicator(5, isClientStatusAllowed, isPrimaryCommandAllowed || isSecondaryCommandAllowed));	// Payload indicator, 5 counters and DeviceSettingPresent

			if (isClientStatusAllowed)
				sb.Append(GetClientStatusResponse(inProtocolVersion, 1));
			
			if (isPrimaryCommandAllowed)
				sb.Append(GetProductAndSerialNumberResponse());
			else if (isSecondaryCommandAllowed)
				sb.Append(GetFirwareVersionResponse());
				
			sb.AppendFormat("87" +				// Group ID = Primary Meter counters and 5 counter records
							"01{0}" +			// Consumed energy tariff I
							"02{1}" +			// Consumed energy tariff II
							"03{2}" +			// Produced energy tariff I
							"04{3}" +			// Produced energy tariff II
							"0500000000" +		// Actual produced power
							"060000012C" +		// Actual consumed power
							"07{4}",			// Gas
							Counters.First(c => c.Id == 181).Value.ToString("X8"),
							Counters.First(c => c.Id == 182).Value.ToString("X8"),
							Counters.First(c => c.Id == 281).Value.ToString("X8"),
							Counters.First(c => c.Id == 282).Value.ToString("X8"),
							Counters.First(c => c.Id == 2421).Value.ToString("X8"));

			return sb.ToString();
		}
	}


	public class SmartS0Meter : Meter
	{
		public override MeterType MeterType { get { return MeterType.SmartS0; } }


		public override byte GetMeterTypeForMessage(bool inIsDuo)
		{
			if (inIsDuo)
				return 30;  // No_Meter

			return 7;		// Smart_Meter_EG
		}


		public override int ActivationMeterId
		{
			get { return 49152; }
		}


		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			UpdateCounters(inTimestamp);
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			var sb = new StringBuilder();

			if (!inIsDuo)
			{
				sb.Append(0x81.ToString("X2")); // Payload indicator, smart meter present.		
				sb.Append(0x71.ToString("X2")); // Group ID, secondary meter counters and 1 counter
				// Payload copied from 12-49-002-882 on PROD.
				string cPayloadFormat = 
					inProtocolVersion < 41 ?
@"01{0:00000000}/ISk5\2MT382-1003
0-0:96.1.1(5A424556303035303838393336313132) 
1-0:1.8.1({1:00000.000}*kWh) 
1-0:1.8.2({2:00000.000}*kWh) 
1-0:2.8.1({3:00000.000}*kWh) 
1-0:2.8.2({4:00000.000}*kWh) 
0-0:96.14.0(0002) 
1-0:1.7.0(0001.86*kW) 
1-0:2.7.0(0000.00*kW) 
0-0:17.0.0(0999.00*kW) 
0-0:96.3.10(1) 
0-0:96.13.1() 
0-0:96.13.0() 
0-1:24.1.0(3) 
0-1:96.1.0(3234313537303032393434363233353132) 
0-1:24.3.0(130819100000)(00)(60)(1)(0-1:24.2.1)(m3) ({5:00000.000}) 
0-1:24.4.0(1) 
! 
" :
@"01{0:00000000}/KFM5KAIFA-METER 1-3:0.2.8(40) 0-0:1.0.0(000102045905W) 
0-0:96.1.1(4530303033303030303030303032343133) 
1-0:1.8.1({1:00000.000}*kWh) 
1-0:1.8.2({2:00000.000}*kWh) 
1-0:2.8.1({3:00000.000}*kWh) 
1-0:2.8.2({4:00000.000}*kWh) 
0-0:96.14.0(0001) 1-0:1.7.0(00.034*kW) 1-0:2.7.0(00.000*kW) 0-0:17.0.0(999.9*kW) 
0-0:96.3.10(1) 0-0:96.7.21(00073) 0-0:96.7.9(00020) 
1-0:99.97.0(3)(0-0:96.7.19)(000124235657W)(0000003149*s)(000124225935W)(0000000289*s)(000101000001W)(2147483647*s) 
1-0:32.32.0(00005) 1-0:52.32.0(00006) 1-0:72.32.0(00001) 
1-0:32.36.0(00000) 1-0:52.36.0(00000) 1-0:72.36.0(00000) 
0-0:96.13.1() 0-0:96.13.0() 1-0:31.7.0(000*A) 1-0:51.7.0(000*A) 
1-0:71.7.0(000*A) 1-0:21.7.0(00.034*kW) 1-0:22.7.0(00.000*kW) 
1-0:41.7.0(00.000*kW) 1-0:42.7.0(00.000*kW) 1-0:61.7.0(00.000*kW) 
1-0:62.7.0(00.000*kW) 0-1:24.1.0(003) 0-1:96.1.0(4730303131303033303832373133363133) 
0-1:24.2.1(000102043601W)({5:00000.000}*m3) 0-1:24.4.0(1) !583C
";
				sb.AppendFormat(cPayloadFormat,
								Counters.FirstOrDefault(c => c.Id == 1).Value,
								Counters.FirstOrDefault(c => c.Id == 181).Value / 1000.0f,
								Counters.FirstOrDefault(c => c.Id == 182).Value / 1000.0f,
								Counters.FirstOrDefault(c => c.Id == 281).Value / 1000.0f,
								Counters.FirstOrDefault(c => c.Id == 282).Value / 1000.0f,
								Counters.FirstOrDefault(c => c.Id == 2421).Value / 1000.0f
								);

				return sb.ToString();
			}

			// IsDuo
			bool isPrimaryCommandAllowed;
			bool isSecondaryCommandAllowed;
			bool isTertiaryCommandAllowed;
			bool isClientStatusAllowed;
			GetCommandsAllowed(inSequenceNumber, inIsDuo, inSendClientStatus, out isPrimaryCommandAllowed, out isSecondaryCommandAllowed, out isTertiaryCommandAllowed, out isClientStatusAllowed);

			sb.Append(GetPayloadIndicator(8, isClientStatusAllowed, isPrimaryCommandAllowed || isSecondaryCommandAllowed));	// Payload indicator, 7 counters + 1 counter for S0 and DeviceSettingPresent

			if (isClientStatusAllowed)
				sb.Append(GetClientStatusResponse(inProtocolVersion, 1));

			if (isPrimaryCommandAllowed)
				sb.Append(GetProductAndSerialNumberResponse());
			else if (isSecondaryCommandAllowed)
				sb.Append(GetFirwareVersionResponse());

			sb.AppendFormat("87" +				// Group ID = Primary Meter counters and 7 counter records
							"01{0}" +			// Consumed energy tariff I
							"02{1}" +			// Consumed energy tariff II
							"03{2}" +			// Produced energy tariff I
							"04{3}" +			// Produced energy tariff II
							"0500000000" +		// Actual produced power
							"060000012C" +		// Actual consumed power
							"07{4}" +			// Gas
							"01" +				// Group id, Secondary, client index 0 and 1 counter
							"01{5}",			// S0 production
							Counters.FirstOrDefault(c => c.Id == 181).Value.ToString("X8"),
							Counters.FirstOrDefault(c => c.Id == 182).Value.ToString("X8"),
							Counters.FirstOrDefault(c => c.Id == 281).Value.ToString("X8"),
							Counters.FirstOrDefault(c => c.Id == 282).Value.ToString("X8"),
							Counters.FirstOrDefault(c => c.Id == 2421).Value.ToString("X8"),
							Counters.FirstOrDefault(c => c.Id == 1).Value.ToString("X8"));

			return sb.ToString();
		}
	}


	public class Soladin : Meter
	{
		public override MeterType MeterType { get { return MeterType.Soladin; } }


		public override byte GetMeterTypeForMessage(bool inIsDuo)
		{
			return 9;		// Soladin_600
		}


		public override int ActivationMeterId
		{
			get { throw new NotImplementedException("ActivationMeterId not set."); }
		}


		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			UpdateCounters(inTimestamp);
			var sb = new StringBuilder();
			sb.Append(0x80.ToString("X2"));			// Payload indicator, smart meter present.

			// Payload copied from 12-27-001-092 on PROD.
			// Characters 40-45 form the three byte counter value.
			const string cPayload = "00001100B6F300008203F7008513E4000000C9006A3D003DE5D00000000014";
			sb.Append(cPayload.Substring(0, 40));
			sb.AppendFormat("{0:X2}", Counters[0].Value & 0xff);
			sb.AppendFormat("{0:X2}", (Counters[0].Value & 0xff00) >> 8);
			sb.AppendFormat("{0:X2}", (Counters[0].Value & 0xff0000) >> 16);
			sb.Append(cPayload.Substring(46));

			return sb.ToString();
		}
	}


	public class NoMeter : Meter
	{
		public override MeterType MeterType { get { return MeterType.NoMeter; } }


		public override byte GetMeterTypeForMessage(bool inIsDuo)
		{
			return 0;
		}

		public override string GetCountersPayload(DateTime inTimestamp, int inProtocolVersion, int inSequenceNumber, bool inIsDuo, bool inSendClientStatus)
		{
			// Return nr counters = 0.
			return "00";
		}

		public override int ActivationMeterId
		{
			get { throw new NotImplementedException(); }
		}
	}


	/// <summary>
	/// Specific Meter class with an extra method to add real data instead of using generated data.
	/// </summary>
	public class VirtualSmartMeter : SmartMeter
	{
		public void AddValue(int inId, float inValue)
		{
			Counters.Add(new RealValueCounter(inId) { Value = (long)inValue });
		}


		private class RealValueCounter : Counter
		{
			public RealValueCounter(int inId)
				: base(inId)
			{
			}


			public override void Update(DateTime inTimestamp)
			{
				// Do nothing, base class generates random value, we don't want that.
			}
		}
	}
}
