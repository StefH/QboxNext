using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{

	public class DeviceSettingsPayload : BasePayload
	{
		private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
		/// <summary>
		/// Parse devicesettings and returns one or a list of DeviceSettings
		/// Number of DeviceSettings is dependent on first DeviceSettingType (setting), SensorSettingsComposite and SensorMeasurements contains multi settings
		/// </summary>
		/// <param name="ProtocolNr">Firmware version 39: clientbyte added by client settings & client info</param>
		/// <param name="setting">DeviceSettingType</param>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static IEnumerable<DeviceSettingsPayload> GetDeviceSettings(int ProtocolNr, DeviceSettingType setting, StringParser parser)
		{
			bool handled = false;

			if (ProtocolNr >= 41)
			{
				handled = true;
				switch (setting)
				{
					case DeviceSettingType.P1CounterOptions: 
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = parser.ParseByte() };
						break;
					case DeviceSettingType.ClientManufacturerReport:  
						yield return new DeviceSettingsPayload
						{
							LastReceived = DateTime.Now.ToUniversalTime(),
							DeviceSetting = setting,
							Client = null,
							DeviceSettingValueStr =
								String.Format("Manufacturer ID {0}, Product type {1}, Product ID {2}",
									parser.ParseInt16(),
									parser.ParseInt16(),
									parser.ParseInt16())
						};
						break;
					case DeviceSettingType.ClientNodeId:
					case DeviceSettingType.ClientNumberOfDevicesInZWaveNetwork:
						parser.ParseByte();
						break;
					case DeviceSettingType.ClientCounterTypesSmartPlug:
						var nbrCounterTypes = parser.ParseByte();
						for (int i = 0; i < nbrCounterTypes; i++)
							parser.ParseByte();
						break;
					case DeviceSettingType.ClientCompositeZWave:  // 0x4D
						yield return ParseCompositeZwave(ProtocolNr, parser);
						break;
					case DeviceSettingType.ClientRawNodeInformation:
					case DeviceSettingType.ClientRawSupportedReport:
						parser.ParseByte();
						parser.ParseDelimitedText('"');
						break;
					case DeviceSettingType.ClientZWaveNetworkInfoList: // 0x60,
						break;
					case DeviceSettingType.ClientZWaveMeterReport: // 0x61,
						break;
					case DeviceSettingType.ClientZwaveBasicSetSetting: // 0x62
						break;
					default:
						handled = false;
						break;
				}
			}

			if ((!handled) && (ProtocolNr >= 44))
			{
				handled = true;
				switch (setting)
				{
					case DeviceSettingType.P1DSMRVersion:
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = parser.ParseByte() };
						break;
					default:
						handled = false;
						break;
				}
			}

			if (!handled)
			{
				switch (setting)
				{
					case DeviceSettingType.ManufacturerMeterType:
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = parser.ParseInt32() };
						break;
					case DeviceSettingType.Sensor1Low:
					case DeviceSettingType.Sensor1High:
					case DeviceSettingType.Sensor2Low:
					case DeviceSettingType.Sensor2High:
					case DeviceSettingType.SensorMinPulseLength:
					case DeviceSettingType.SensorMinPulseWidth:
					case DeviceSettingType.SensorMaxPulseWidth:
					case DeviceSettingType.SensorMinGap:
					case DeviceSettingType.SensorMaxGap:
					case DeviceSettingType.SensorNoiseLevel:
					case DeviceSettingType.SensorFilterCoefficient:
					case DeviceSettingType.SensorPulseLengthMin:
					case DeviceSettingType.SensorPulseLengthMax:
					case DeviceSettingType.SensorPulseWidthMin:
					case DeviceSettingType.SensorPulseWidthMax:
					case DeviceSettingType.SensorBaselineMin:
					case DeviceSettingType.SensorBaselineMax:
					case DeviceSettingType.SensorSignalMin:
					case DeviceSettingType.SensorSignalMax:
					case DeviceSettingType.SensorSignalAverage:
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = parser.ParseUInt16() };
						break;
					case DeviceSettingType.PrimaryMeterType:
					case DeviceSettingType.SecondaryMeterType:
					case DeviceSettingType.SensorChannel:
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = parser.ParseByte() };
						break;
					case DeviceSettingType.CalibrationSettingsComposite:
						for (byte i = 0; i < 4; i++)
						{
							yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = (DeviceSettingType)parser.ParseByte(), DeviceSettingValue = parser.ParseUInt16() };
						}
						break;
					case DeviceSettingType.SensorSettingsComposite:
						for (byte i = 0; i < 7; i++)
						{
							yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = (DeviceSettingType)parser.ParseByte(), DeviceSettingValue = parser.ParseUInt16() };
						}
						break;
					case DeviceSettingType.SensorMeasurements:
						for (int i = 0; i < 9; i++)
						{
							yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = (DeviceSettingType)parser.ParseByte(), DeviceSettingValue = parser.ParseUInt16() };
						}
						break;
					case DeviceSettingType.FirmwareUrl:
					case DeviceSettingType.ReportUrl:
					case DeviceSettingType.P1ManufacturerCode:
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValueStr = parser.ParseDelimitedText('"') };
						break;
					case DeviceSettingType.ClientProductAndSerialNumber:
					case DeviceSettingType.ClientRawP1Data:
						yield return new DeviceSettingsPayload
						{
							LastReceived = DateTime.Now.ToUniversalTime(),
							DeviceSetting = setting,
							Client = ProtocolNr >= 39 ? (QboxClient?)parser.ParseByte() : null,
							DeviceSettingValueStr = parser.ParseDelimitedText('"')
						};
						break;
					case DeviceSettingType.ClientPrimaryMeterType:
					case DeviceSettingType.ClientSecondaryMeterType:
					case DeviceSettingType.ClientFirmwareVersion:
						yield return new DeviceSettingsPayload
						{
							LastReceived = DateTime.Now.ToUniversalTime(),
							DeviceSetting = setting,
							Client = ProtocolNr >= 39 ? (QboxClient?)parser.ParseByte() : null,
							DeviceSettingValue = parser.ParseByte()
						};
						break;
					default:
						throw new Exception(String.Format("Unknown device setting: {0}", setting));
				}
			}
		}

		/// <summary>
		/// At this moment the bytes are read(parsed) and return as a string.
		/// </summary>
		private static DeviceSettingsPayload ParseCompositeZwave(int ProtocolNr, StringParser parser)
		{
			var setting = parser.ParseByte();
			if (setting != 0x47)
				throw new ArgumentOutOfRangeException(String.Format("By 0x4D (Composite Zwave) Expected byte is: 0x47 found: {0:x2}", setting));
			var data = new ZWaveNetworkInfo();
			data.NumberOfDevices = parser.ParseByte();
			var nbrOfClientInfo = parser.ParseByte();
			for (int i = 0; i < nbrOfClientInfo; i++)
			{
				data.ClientInfos.Add(new ZWaveClientInfo
				{
					Info = parser.ParseDelimitedText('"')
				});
			}

			setting = parser.ParseByte();
			if (setting != (byte)DeviceSettingType.ClientZWaveNetworkInfoList)
				throw new ArgumentOutOfRangeException(String.Format("By 0x4D (Composite Zwave) Expected byte after client info's is: 0x60 found: {0:x2}", setting));

			var nbrOfDevices = parser.ParseByte();
			for (int i = 0; i < nbrOfDevices; i++)
			{
				data.Devices.Add(new ZWaveNetworkDevice
				{
					NodeId = parser.ParseByte(),
					DeviceType = parser.ParseByte()
				});
			}
			
			var deviceSettingValueStr = JsonConvert.SerializeObject(data, 
				new JsonSerializerSettings() {
					Converters = new JsonConverter[] { new StringEnumConverter() },
					Formatting = Formatting.Indented}
				);
			return new DeviceSettingsPayload
			{
				LastReceived = DateTime.Now.ToUniversalTime(),
				Client = null,
				DeviceSettingValueStr = deviceSettingValueStr
			};
		}

		public DeviceSettingType DeviceSetting { get; set; }
		/// <summary>
		/// Client introduced in firmware 39, value = null by firmware before 39
		/// </summary>
		public QboxClient? Client { get; set; }
		private int DeviceSettingValue { set { DeviceSettingValueStr = value.ToString(); } }
		public string DeviceSettingValueStr { get; set; }
		public DateTime LastReceived { get; private set; }

		public override void Visit(IVisitor visitor)
		{
			Log.Trace("Enter");
			visitor.Accept(this);
			Log.Trace("Exit");
		}
	}
}

