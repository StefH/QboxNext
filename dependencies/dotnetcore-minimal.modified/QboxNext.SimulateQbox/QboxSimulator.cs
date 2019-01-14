using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NLog;
using QboxNext.Core.Simulation;
using QboxNext.Core.Utils;
using System.Threading;
using System.IO;
using System.Net;
using System.Globalization;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.SimulateQbox
{
	public enum SimulatorState
	{
		Uninitialized,
		Initializing,
		Stopped,
		Playing
	}


	public delegate void GenericEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);


	public class QboxSimulator : IDisposable
	{
		private static readonly Logger Logger = LogManager.GetLogger("QboxSimulator");
		private static readonly DateTime Epoch = new DateTime(2007, 1, 1); 
		StringBuilder _log = new StringBuilder();
		TimeSpan _messageInterval = new TimeSpan(0, 1, 0);
		string _host;
		int _currentSequenceNr = 1;		// Don't truncate this to a byte, we want to have continuously increasing sequence numbers to feed into ComputeCounterXXX.

		private bool _LoadPersistedSequenceNr = true;
		public bool LoadPersistedSequenceNr
		{
			get { return _LoadPersistedSequenceNr; }
			set { _LoadPersistedSequenceNr = value; }
		}

		private bool _RunContinuously = true;
		public bool RunContinuously
		{
			get { return _RunContinuously; }
			set { _RunContinuously = value; }
		}

		public DateTime				StartTime					{ get; set; }

		private int _Timeout = 30000;
		public int					Timeout
		{
			get { return _Timeout; }
			set { _Timeout = value; }
		}

		public TimeSpan				Offset						{ get; set; }
		public string				QboxSerial					{ get; set; }
		public string				QboxDeviceName				{ get; set; }
		public bool					QboxIsDuo					{ get; set; }
		public Meter				Meter						{ get; private set; }
		public bool					SendClientStatus			{ get; set; }
		public int					ProtocolVersion				{ get; set; }

		public int					MessageSentCount			{ get; private set; }
		public int					FailedCount					{ get; private set; }
		public int					SuccessfulResponseCount		{ get; private set; }
		public SimulatorState		State						{ get; private set; }
		public bool					LastMessageFailed			{ get; private set; }
		public DateTime				LastMessageSend				{ get; private set; }
		public TimeSpan				ResponseTime				{ get; private set; }
		public TimeSpan				MaxResponseTime				{ get; private set; }
		public TimeSpan				MinResponseTime				{ get; private set; }
		public bool					Disposing					{ get; private set; }


		public static volatile int NrMessagesSending;
		private static Object _nrMessagesSendingLock = new Object();


		private QboxState _qboxState = QboxState.Operational;
		public QboxState QboxState
		{
			get { return _qboxState; }
			set { _qboxState = value; }
		}

		private List<UsagePatternSpec> _usagePatterns;


		public event GenericEventHandler<QboxSimulator, EventArgs> OnLog;


		public QboxSimulator(string inSerial)
		{
			ProtocolVersion = 0x28;
			QboxSerial = inSerial;
			State = SimulatorState.Uninitialized;
			LogAction("Created");
		}


		public QboxSimulator(int serialNumberSuffix)
			: this(string.Format("99-95-{0:000}-{1:000}", serialNumberSuffix / 1000, serialNumberSuffix % 1000))
		{
		}


		private string ResidenceId
		{
			get
			{
				// 99-95-001-999 -> 995001999
				// Remove all dashes
				// Strip off the first digit (otherwise we end up with a number > int.MaxValue)
				var numberText = QboxSerial.Replace("-", "");
				numberText = numberText.Substring(1);

				int dummy;
				Guard.IsTrue(int.TryParse(numberText, out dummy), "Can't generate ResidenceId from QboxSerial " + QboxSerial);

				return numberText;
			}
		}


		/// <summary>
		/// Start and run the simulator on the current thread.
		/// </summary>
		public void Start()
		{
			LogAction("Start");
			if (_LoadPersistedSequenceNr)
				LoadSequenceNr();
			Simulate();
		}


		public string Log
		{
			get
			{
				return _log.ToString();
			}
		}


		public void Stop()
		{
			LogAction("Stop");
			SaveSequenceNr();
			State = SimulatorState.Stopped;
		}


		private void Simulate()
		{
			State = SimulatorState.Playing;

			var nextStartTime = StartTime + Offset;
			// Start at the next possible minute.
			while (nextStartTime < DateTime.Now)
				nextStartTime = nextStartTime.AddMinutes(1);
			Logger.Info("{0} starting at {1}", QboxSerial, nextStartTime);

			while (State == SimulatorState.Playing)
			{
				// Determine how long to wait before we have to send a message.
				var startInMs = (int)(nextStartTime - DateTime.Now).TotalMilliseconds;

				if (startInMs <= 0)
				{
					SendMessage();
					nextStartTime = nextStartTime.AddMinutes(1);
				}
				else
				{
					Thread.Sleep(Math.Min(startInMs, 1000));
				}
			}
		}


		private void SendMessage()
		{
			if (State == SimulatorState.Stopped)
				return;

			lock (_nrMessagesSendingLock)
				NrMessagesSending++;

			try
			{
				// Onthoud wanneer we begonnen zijn met het versturen van het bericht, zodat we kunnen
				// uitrekenen hoe lang het duurde (LogAction doet dat).
				var timestampStart = DateTime.Now;
				try
				{
					MessageSentCount++;
					string responseMsg = SendMessageToServer().WithoutStxEtxEnvelope();
					var responseSequenceNr = Convert.ToInt32(responseMsg.Substring(0, 2), 16);
					var responseServerTimeSeconds = Convert.ToUInt32(responseMsg.Substring(2, 8), 16);
					var serverTime = Epoch.AddSeconds(responseServerTimeSeconds);
					_currentSequenceNr++;
					SuccessfulResponseCount++;
					LogAction(String.Format("<-- {0} (seq# {1}, servertime {2:HH:mm:ss})", responseMsg, responseSequenceNr, serverTime), timestampStart, LogType.Success);
					var millisecondsDrift = Math.Abs((DateTime.Now - serverTime).TotalMilliseconds);
					if (millisecondsDrift > 1000)
						LogAction(String.Format("    Warning: clocks misaligned by {0} ms", millisecondsDrift), DateTime.Now, LogType.Failed);
					HandleResponse(responseMsg);
				}
				catch (WebException exception)
				{
					LogAction(exception.Message, timestampStart, LogType.Failed);
				}

				if (!RunContinuously)
					Stop();
			}
			finally
			{
				lock (_nrMessagesSendingLock)
					NrMessagesSending--;
			}
		}


		private void LogAction(string text, DateTime startTime, LogType? action)
		{
			LogAction(text, DateTime.Now - startTime, action);
		}


		private void LogAction(string text, TimeSpan? responseTime = null, LogType? action = null)
		{
			const int cResponseTimeWidth = 8;
			var responseTimeString = new string(' ', cResponseTimeWidth);

			if (responseTime != null)
			{
				responseTimeString = responseTime.Value.TotalMilliseconds.ToString("0") + " ms";
				responseTimeString = responseTimeString.PadLeft(cResponseTimeWidth);
			}

			LastMessageSend = DateTime.Now;

			string message = string.Format("[{0}] {1} {2}  {3}", LastMessageSend.ToString("HH:mm:ss"), QboxSerial, responseTimeString, text);
			_log.AppendLine(message);
			Logger.Info(message);

			if (action.HasValue)
			{
				if (action == LogType.Success)
					LastMessageFailed = false;
				else if (action == LogType.Failed)
					LastMessageFailed = true;
			}

			if (responseTime != null)
			{
				ResponseTime = responseTime.Value;

				if (ResponseTime < MinResponseTime || MinResponseTime == TimeSpan.Zero)
					MinResponseTime = ResponseTime;
				if (ResponseTime > MaxResponseTime)
					MaxResponseTime = ResponseTime;
			}

			if (action == LogType.Failed)
				FailedCount++;

			// prevents race condition
			var onlogEvent = OnLog;

			if (onlogEvent != null)
				onlogEvent(this, EventArgs.Empty);
		}


		/// <summary>
		/// Send a counter message to the Qserver.
		/// </summary>
		private string SendMessageToServer()
		{
			string messageString = BuildMessageString();
			LogAction("--> " + messageString);
			byte[] data = Encoding.ASCII.GetBytes(messageString);

			var stream = HttpPost(Host + "/device/qbox/" + QboxDeviceName + "/" + QboxSerial, data, "text/html");
			if (stream == null)
				throw new Exception("No ResponseStream"); // FIXME: needs more specific Exception

			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}


		/// <summary>
		/// Build the message string from one or more counters.
		/// </summary>
		string BuildMessageString()
		{
			var msgBuilder = new QboxMessageBuilder(Meter, QboxIsDuo, SendClientStatus);
			msgBuilder.QboxState = QboxState;
			return msgBuilder.BuildMessageString(ProtocolVersion, _currentSequenceNr, DateTime.Now);
		}


		/// <summary>
		/// Post content to the specified URI.
		/// </summary>
		/// <returns>Stream containing the response.</returns>
		Stream HttpPost(string uri, byte[] content, string contentType)
		{
			var webRequest = WebRequest.Create(uri);

			webRequest.Timeout = Timeout;

			webRequest.ContentType = contentType;
			webRequest.Method = "POST";
			var bytes = content;
			webRequest.ContentLength = bytes.Length;
			var os = webRequest.GetRequestStream();
			os.Write(bytes, 0, bytes.Length);
			os.Close();
			var resp = webRequest.GetResponse();
			return resp.GetResponseStream();
		}


		/// <summary>
		/// Handle the response of the server.
		/// </summary>
		void HandleResponse(string inResponse)
		{
			// If the response contains a space, it's probably an error, so ignore it.
			if (inResponse.Contains(" "))
				return;

			string unwrappedResponse = inResponse.WithoutStxEtxEnvelope();
			// First 12 characters are normal response, the rest are commands.
			// TODO: correctly handle new offset.
			if (unwrappedResponse.Length > 12)
			{
				string commands = unwrappedResponse.Substring(12);
				while (!string.IsNullOrEmpty(commands))
				{
					string commandString = commands.Substring(0, 2);
					int command = Convert.ToInt32(commandString, 16);
					string arguments = commands.Substring(2);
					switch (command)
					{
						case (int)DeviceSettingType.PrimaryMeterType:
							commands = HandleSetMeterType(arguments);
							break;
						case (int)DeviceSettingType.SensorChannel:
							commands = HandleSetSensorChannel(arguments);
							break;
						case 0x80:
							commands = HandleUpgradeFirmware(arguments);
							break;
						case 0x81:
							commands = HandleRestart(arguments);
							break;
						case 0x82:
							commands = HandleRestartFactoryDefaults(arguments);
							break;
						case 0xA2:
							commands = HandleRestartClientFactoryDefaults(arguments);
							break;
						case 0x84:
							commands = HandleCalibrateMeter(arguments);
							break;
						case 0x85:
							commands = HandleDeviceSettings(arguments);
							break;
						case 0xA3:
							commands = HandleClientProductAndSerialNumberRequest(arguments);
							break;
						case 0xA6:
							commands = HandleClientFirmwareRequest(arguments);
							break;
						default:
							LogAction(string.Format("Command: unknown command 0x{0}, arguments 0x{1}", command.ToString("X2"), arguments));
							// We can't determine the end of the command, so we just skip the rest of it.
							commands = string.Empty;
							break;
					}
				}
			}
		}


		/// <summary>
		/// Handle the command Restart.
		/// </summary>
		/// <returns>
		/// The commands that follow this command.
		/// </returns>
		string HandleRestart(string inArguments)
		{
			LogAction("Command: restart");
			return inArguments;
		}


		/// <summary>
		/// Handle the command Upgrade Firmware.
		/// </summary>
		/// <returns>
		/// The commands that follow this command.
		/// </returns>
		private string HandleUpgradeFirmware(string inArguments)
		{
			LogAction("Command: upgrade firmware");
			return inArguments;
		}


		/// <summary>
		/// Handle the command Calibrate Meter.
		/// </summary>
		/// <returns>
		/// The commands that follow this command.
		/// </returns>
		string HandleCalibrateMeter(string inArguments)
		{
			byte minutes = ParseHexByte(inArguments.Substring(0, 2));
			LogAction(String.Format("Command: calibrate meter during {0} minutes", minutes));
			return inArguments.Substring(2);
		}


		/// <summary>
		/// Handle the command Set Sensor Channel.
		/// </summary>
		/// <returns>
		/// The commands that follow this command.
		/// </returns>
		string HandleSetSensorChannel(string inArguments)
		{
			byte channel = ParseHexByte(inArguments.Substring(0, 2));
			LogAction("Command: set channel to " + channel);
			return inArguments.Substring(2);
		}


		/// <summary>
		/// Handle the command Set Meter Type.
		/// </summary>
		/// <returns>
		/// The commands that follow this command.
		/// </returns>
		string HandleSetMeterType(string inArguments)
		{
			var meterType = GetMeterTypeFromId(inArguments.Substring(0, 2));
			
			LogAction("Command: set meter type " + meterType);
			if (meterType != MeterType.NoMeter)
			{
				SetMeter(meterType, _usagePatterns);

				QboxState = QboxState.Operational;
			}

			return inArguments != string.Empty ? inArguments.Substring(2) : string.Empty;
		}


		/// <summary>
		/// Handle the command 'Restart with factory defaults'.
		/// </summary>
		/// <returns>
		/// The commands that follow this command.
		/// </returns>
		string HandleRestartFactoryDefaults(string inArguments)
		{
			LogAction("Command: Restart with factory defaults");
			QboxState = QboxState.Waiting;
			
			return inArguments;
		}


		/// <summary>
		/// Handle the command 'Restart client with factory defaults'.
		/// </summary>
		/// <returns>
		/// The commands that follow this command.
		/// </returns>
		string HandleRestartClientFactoryDefaults(string inArguments)
		{
			var clientId = ParseHexByte(inArguments.Substring(0, 2));
			LogAction(string.Format("Command: Restart client {0} with factory defaults", clientId));
			return inArguments != string.Empty ? inArguments.Substring(2) : string.Empty;
		}


		/// <summary>
		/// Handle the command 'Get device settings'.
		/// </summary>
		/// <param name="inArguments">A sequence of device settings.</param>
		/// <returns>The commands that follow this command.</returns>
		string HandleDeviceSettings(string inArguments)
		{
			LogAction((string.Format("Command: Handle device setting")));

			var deviceSetting = ParseHexByte(inArguments.Substring(0, 2));

			switch (deviceSetting)
			{
				case 0x20:
					LogAction("Device setting: Get LED sensor Channel");
					break;
				case 0x0A:
					LogAction("Device setting: Get Ferraris Callibration Composite");
					break;
				default:
					LogAction(string.Format("Device setting: unknown device setting 0x{0}", deviceSetting.ToString("X2")));
					break;
			}

			return inArguments != string.Empty ? inArguments.Substring(2) : string.Empty;
		}


		/// <summary>
		/// Handle the command 'Request client personal and serial number'
		/// </summary>
		string HandleClientProductAndSerialNumberRequest(string inArguments)
		{
			byte clientId = ParseHexByte(inArguments.Substring(0, 2));
			LogAction("Command: Get product and serial number from client " + clientId);
			return inArguments != string.Empty ? inArguments.Substring(2) : string.Empty;
		}


		/// <summary>
		/// Handle the command 'Request client firmware version'
		/// </summary>
		string HandleClientFirmwareRequest(string inArguments)
		{
			byte clientId = ParseHexByte(inArguments.Substring(0, 2));
			LogAction("Command: Get firmware version from client " + clientId);
			return inArguments != string.Empty ? inArguments.Substring(2) : string.Empty;
		}


		/// <summary>
		/// Set the meter to a meter with the specified behaviour.
		/// </summary>
		public void SetMeter(MeterType inMeterType, List<UsagePatternSpec> inUsagePatterns)
		{
			if (Meter != null)
				LogAction("Changing to meter type " + inMeterType);
			Meter = Meter.CreateMeter(inMeterType);
			_usagePatterns = inUsagePatterns;
			Meter.SetUsagePatterns(inUsagePatterns);
		}


		/// <summary>
		/// Determine meter type from hex ID in command.
		/// </summary>
		MeterType GetMeterTypeFromId(string inHexId)
		{
			var meterTypeId = Convert.ToInt32(inHexId, 16);
			switch (meterTypeId)
			{
				case 0:
				case 1:
					return MeterType.Led;
				case 2:
					return MeterType.Ferraris;
				case 6:
				case 7:
					return MeterType.Smart;
				case 9:
					return MeterType.Soladin;
				case 0x1E:
					return MeterType.NoMeter;
				default:
					throw new NotImplementedException("Unknown meter type " + inHexId);
			}
		}


		/// <summary>
		/// Persist the current sequence number of the current Qbox.
		/// </summary>
		void SaveSequenceNr()
		{
			var dataPath = SimulateQboxDataPath;
			var dataDir = Path.GetDirectoryName(dataPath);
			if (!Directory.Exists(dataDir))
				Directory.CreateDirectory(dataDir);
			File.WriteAllText(dataPath, string.Format("SequenceNr={0}", _currentSequenceNr));
		}


		/// <summary>
		/// Load persisted sequence number for the current Qbox.
		/// </summary>
		void LoadSequenceNr()
		{
			if (File.Exists(SimulateQboxDataPath))
			{
				var data = File.ReadAllText(SimulateQboxDataPath);
				_currentSequenceNr = int.Parse(data.Split('=')[1], CultureInfo.InvariantCulture);
			}
		}


		/// <summary>
		/// Path to store the information of the current Qbox.
		/// </summary>
		string SimulateQboxDataPath
		{
			get { return AppDomain.CurrentDomain.BaseDirectory + string.Format(@"\data\{0}.dat", QboxSerial); }
		}


		public override string ToString()
		{
			return string.Format("QboxSimulator {0}", QboxSerial);
		}


		public TimeSpan MessageInterval
		{
			get { return _messageInterval; }
			set { _messageInterval = value; }
		}


		public string Host
		{
			get { return _host; }
			set
			{
				// Strip EndSlash if it was entered
				_host = value.TrimEnd('/');
			}
		}


		public bool Activated
		{
			get
			{
				if (State == SimulatorState.Playing || State == SimulatorState.Stopped)
					return true;
				else
					return false;
			}
		}

		private enum LogType
		{
			Success, Failed
		}

		public void Dispose()
		{
			Stop();

			Disposing = true;
		}


		/// <summary>
		/// Parse inHexString as a hex byte, for example "FF" will return 255.
		/// </summary>
		private static byte ParseHexByte(string inHexString)
		{
			Debug.Assert(inHexString.Length == 2);
			return (byte)Convert.ToInt32(inHexString, 16);
		}
	}
}
