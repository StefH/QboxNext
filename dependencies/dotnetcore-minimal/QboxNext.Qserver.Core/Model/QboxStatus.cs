using System.Linq;
using System;
using System.Collections.Generic;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qserver.Core.Model
{
    //key: pn/sn

    //fwVersion: 15,
    //ip: {
    //    <ip>: Date(),
    //},
    //server: <url>,
    //debug: {
    //    <settings>
    //},
    //state: <state byte>,
    //lastSeen: Date(),
    //lastInvalidResponse: Date(),
    //lastTimeUnreliable: Date(),
    //lastNotOperational: Date(),
    //lastHardReset: Date(),
    //lastPowerLoss: Date(),
    //lastImageValid: Date(),
    //lastImageInvalid: Date(),
    //activated: Date(),

    public class ClientStatePoco
    {
        public bool ConnectionWithClient { get; set; }
        public bool ActionsFailed { get; set; }
        public ClientState State { get; set; }
    }

    public enum ResubmitState
    {
        None,
        Resubmitting,
        Done,
        Error
    }

    /// <summary>
    /// POCO Status object for storage in the Repository.
    /// This acts as a data holder.
    /// </summary>
    public class QboxStatus
    {
        public string Id { get; set; }
        public int FirmwareVersion { get; set; }
        public Dictionary<string, DateTime> IpAddress { get; set; }
	    public KeyValuePair<string, DateTime> LastIpAddress
	    {
		    get
		    {
			    var lastIpAddress = string.Empty;
				var lastIpAddressUpdate = DateTime.MinValue;
				foreach (var keyValue in IpAddress)
				{
					if (keyValue.Value > lastIpAddressUpdate)
					{
						lastIpAddress = keyValue.Key;
						lastIpAddressUpdate = keyValue.Value;
					}
				}

				return new KeyValuePair<string, DateTime>(lastIpAddress, lastIpAddressUpdate);
		    }
	    }
        public string Url { get; set; }
        public ResubmitState ResubmitState { get; set; }


		/// <summary>
		/// Extra status information.
		/// </summary>
		public string ResubmitStatusString { get; set; }

		/// <summary>
		/// Device settings.
		/// </summary>
        public Dictionary<string, string> DebugSettings { get; set; }

		/// <summary>
		/// Timestamps of last time each device setting was received.
		/// </summary>
		public Dictionary<string, DateTime> DebugSettingsLastReceived { get; set; }

		/// <summary>
		/// State byte as reported in the last Qbox message.
		/// </summary>
        public byte State { get; set; }

        
		/// <summary>
		/// Last time the Qbox has reported to the server.
		/// </summary>
		public DateTime LastSeen { get; set; }												// UTC


		/// <summary>
		/// Last time data for a valid counter received.
		/// </summary>
		public DateTime LastDataReceived { get; set; }										// UTC


	    public Dictionary<string, DateTime> LastValidDataReceivedPerCounter { get; set; }	// UTC
		public DateTime LastElectricityConsumptionSeen { get; set; }						// UTC
		public DateTime LastElectricityGenerationSeen { get; set; }							// UTC
		public DateTime LastGasConsumptionSeen { get; set; }								// UTC
        public DateTime LastValidResponse { get; set; }										// UTC
        public DateTime LastInvalidResponse { get; set; }									// UTC
        public DateTime LastTimeIsReliable { get; set; }									// UTC
        public DateTime LastTimeUnreliable { get; set; }									// UTC


		/// <summary>
		/// Last time the Qbox has reported itself as 'not operational'.
		/// </summary>
        public DateTime LastNotOperational { get; set; }									// UTC
        public DateTime LastHardReset { get; set; }											// UTC
        public DateTime LastPowerLoss { get; set; }											// UTC
        public DateTime LastImageValid { get; set; }										// UTC
        public DateTime LastImageInvalid { get; set; }										// UTC
		public DateTime LastTimeSynced { get; set; }										// UTC
		public DateTime LastPeak { get; set; }												// UTC

        public DateTime LastError { get; set; }												// UTC
        public string LastErrorMessage { get; set; }


        /// <summary>
        /// ClientState dictionary &lt;string, byte&gt;  string = Client (QboxClient enum), byte = ClientState
        /// </summary>
        public Dictionary<string, byte> ClientStatuses { get; private set; }


        /// <summary>
        /// Date of last client-state
        /// Key = string = [Client (QboxClient enum)] + '-' +  [ClientState (ClientMiniStatus enum)]
        /// Value = MeasurementTime
        /// </summary>
        public Dictionary<string, DateTime> ClientStateDates { get; private set; }

        public QboxStatus()
        {
            IpAddress = new Dictionary<string, DateTime>();
            DebugSettings = new Dictionary<string, string>();
			DebugSettingsLastReceived = new Dictionary<string, DateTime>();
			LastValidDataReceivedPerCounter = new Dictionary<string, DateTime>();
            ClientStatuses = new Dictionary<string, byte>();
            ClientStateDates = new Dictionary<string, DateTime>();
        }


        public string LastMiniDeviceSetting(DeviceSettingType type)
        {
            if (DebugSettings.ContainsKey(type.ToString()))
            {
                return DebugSettings[type.ToString()];    
            }
            return null;
        }


		/// <summary>
		/// Last time when the device settings of the specified type were received.
		/// </summary>
		/// <returns>Timestamp in server local time.</returns>
		public DateTime LastMiniDeviceSettingLastReceived(DeviceSettingType type)
        {
            if (DebugSettingsLastReceived.ContainsKey(type.ToString()))
                return DebugSettingsLastReceived[type.ToString()].ToLocalTime();    

            return DateTime.MinValue;
        }


		/// <summary>
		/// Get the last time when any of the counters received valid data.
		/// </summary>
		/// <returns>Timestamp in server local time.</returns>
		public DateTime LastValidDataReceived
		{
			get
			{
				return LastValidDataReceivedPerCounter != null &&
					LastValidDataReceivedPerCounter.Count > 0 ? LastValidDataReceivedPerCounter.Values.Max().ToLocalTime() : DateTime.MinValue;
			}
		}


        public string Sensor1Values
        {
            get
            {
                var sensorLow = LastMiniDeviceSetting(DeviceSettingType.Sensor1Low);
                var sensorHigh = LastMiniDeviceSetting(DeviceSettingType.Sensor1High);

                return string.Format("{0}/{1}", (sensorLow ?? "N.A."), (sensorHigh ?? "N.A."));
            }
        }


        public string Sensor2Values
        {
            get
            {
                var sensorLow = LastMiniDeviceSetting(DeviceSettingType.Sensor2Low);
                var sensorHigh = LastMiniDeviceSetting(DeviceSettingType.Sensor2High);

                return string.Format("{0}/{1}", (sensorLow ?? "N.A."), (sensorHigh ?? "N.A."));
            }
        }


        public string LedActiveChannel
        {
            get
            {
                var data = LastMiniDeviceSetting(DeviceSettingType.SensorChannel);
                return (data ?? "N.A.");
            }
        }


	    public DateTime LastLedActiveChannelReceived
	    {
		    get 
			{ 
				return LastMiniDeviceSettingLastReceived(DeviceSettingType.SensorChannel);
		    }
	    }


        public string LedPulseLength
        {
            get
            {
                var data = LastMiniDeviceSetting(DeviceSettingType.SensorMinPulseLength);
                return (data ?? "N.A.");
            }
        }


        public string LedPulseWidth
        {
            get
            {
                var widthMin = LastMiniDeviceSetting(DeviceSettingType.SensorMinPulseWidth);
                var widthMax = LastMiniDeviceSetting(DeviceSettingType.SensorMaxPulseWidth);

                return string.Format("{0}/{1}", (widthMin ?? "N.A."), (widthMax ?? "N.A."));
            }
        }


        public string LedPulseGap
        {
            get
            {
                var gapMin = LastMiniDeviceSetting(DeviceSettingType.SensorMinGap);
                var gapMax = LastMiniDeviceSetting(DeviceSettingType.SensorMaxGap);

                return string.Format("{0}/{1}", (gapMin ?? "N.A."), (gapMax ?? "N.A."));
            }
        }


        public string LedNoiseLevel
        {
            get
            {
                var data = LastMiniDeviceSetting(DeviceSettingType.SensorNoiseLevel);
                return (data ?? "N.A.");
            }
        }


        public string LedFilterCoefficient
        {
            get
            {
                var data = LastMiniDeviceSetting(DeviceSettingType.SensorFilterCoefficient);
                return (data ?? "N.A.");
            }
        }


		public DateTime LastFerrarisActiveSensorOneLowValueReceived
		{
			get
			{
				return LastMiniDeviceSettingLastReceived(DeviceSettingType.Sensor1Low);
			}
		}


		public DateTime LastFerrarisActiveSensorOneHighValueReceived
		{
			get
			{
				return LastMiniDeviceSettingLastReceived(DeviceSettingType.Sensor1High);
			}
		}


		public DateTime LastFerrarisActiveSensorTwoLowValueReceived
		{
			get
			{
				return LastMiniDeviceSettingLastReceived(DeviceSettingType.Sensor2Low);
			}
		}


		public DateTime LastFerrarisActiveSensorTwoHighValueReceived
		{
			get
			{
				return LastMiniDeviceSettingLastReceived(DeviceSettingType.Sensor2High);
			}
		}

	    public string ClientFirmwareVersion
	    {
		    get
		    {
			    var data = LastMiniDeviceSetting(DeviceSettingType.ClientFirmwareVersion);
			    return (data ?? "N.A.");
		    }
	    }


	    public string ClientProductAndSerialNumber
	    {
		    get
		    {
				var data = LastMiniDeviceSetting(DeviceSettingType.ClientProductAndSerialNumber);
				return (data ?? "N.A.");
		    }
	    }


		public DateTime LastClientFirmwareVersionReceived
		{
			get
			{
				return LastMiniDeviceSettingLastReceived(DeviceSettingType.ClientFirmwareVersion);
			}
		}


		public DateTime LastClientProductAndSerialNumberReceived
		{
			get
			{
				return LastMiniDeviceSettingLastReceived(DeviceSettingType.ClientProductAndSerialNumber);
			}
		}
    }
}
