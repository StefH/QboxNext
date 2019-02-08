using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QboxNext.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class DeviceSettingsPayload : BasePayload
	{
	    private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger<DeviceSettingsPayload>();

        /// <summary>
        /// Parse devicesettings and returns one or a list of DeviceSettings
        /// Number of DeviceSettings is dependent on first DeviceSettingType (setting), SensorSettingsComposite and SensorMeasurements contains multi settings
        /// </summary>
        /// <param name="ProtocolNr">Firmware version 39: clientbyte added by client settings & client info</param>
        /// <param name="setting">DeviceSettingType</param>
        /// <param name="reader">The protocol reader.</param>
        /// <returns></returns>
        public static IEnumerable<DeviceSettingsPayload> GetDeviceSettings(int ProtocolNr, DeviceSettingType setting, IProtocolReader reader)
		{
			bool handled = false;

			if (ProtocolNr >= 41)
			{
				handled = true;
				switch (setting)
				{
					case DeviceSettingType.P1CounterOptions: 
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = reader.ReadByte() };
						break;
					case DeviceSettingType.ClientManufacturerReport:  
						yield return new DeviceSettingsPayload
						{
							LastReceived = DateTime.Now.ToUniversalTime(),
							DeviceSetting = setting,
							Client = null,
							DeviceSettingValueStr =
								String.Format("Manufacturer ID {0}, Product type {1}, Product ID {2}",
									reader.ReadInt16(),
									reader.ReadInt16(),
									reader.ReadInt16())
						};
						break;
					case DeviceSettingType.ClientNodeId:
					case DeviceSettingType.ClientNumberOfDevicesInZWaveNetwork:
						reader.ReadByte();
						break;
					case DeviceSettingType.ClientCounterTypesSmartPlug:
						var nbrCounterTypes = reader.ReadByte();
						for (int i = 0; i < nbrCounterTypes; i++)
							reader.ReadByte();
						break;
					case DeviceSettingType.ClientCompositeZWave:  // 0x4D
						yield return ParseCompositeZwave(ProtocolNr, reader);
						break;
					case DeviceSettingType.ClientRawNodeInformation:
					case DeviceSettingType.ClientRawSupportedReport:
						reader.ReadByte();
						reader.ReadEncapsulatedString('"');
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
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = reader.ReadByte() };
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
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = reader.ReadInt32() };
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
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = reader.ReadUInt16() };
						break;
					case DeviceSettingType.PrimaryMeterType:
					case DeviceSettingType.SecondaryMeterType:
					case DeviceSettingType.SensorChannel:
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValue = reader.ReadByte() };
						break;
					case DeviceSettingType.CalibrationSettingsComposite:
						for (byte i = 0; i < 4; i++)
						{
							yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = (DeviceSettingType)reader.ReadByte(), DeviceSettingValue = reader.ReadUInt16() };
						}
						break;
					case DeviceSettingType.SensorSettingsComposite:
						for (byte i = 0; i < 7; i++)
						{
							yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = (DeviceSettingType)reader.ReadByte(), DeviceSettingValue = reader.ReadUInt16() };
						}
						break;
					case DeviceSettingType.SensorMeasurements:
						for (int i = 0; i < 9; i++)
						{
							yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = (DeviceSettingType)reader.ReadByte(), DeviceSettingValue = reader.ReadUInt16() };
						}
						break;
					case DeviceSettingType.FirmwareUrl:
					case DeviceSettingType.ReportUrl:
					case DeviceSettingType.P1ManufacturerCode:
						yield return new DeviceSettingsPayload { LastReceived = DateTime.Now.ToUniversalTime(), DeviceSetting = setting, DeviceSettingValueStr = reader.ReadEncapsulatedString('"') };
						break;
					case DeviceSettingType.ClientProductAndSerialNumber:
					case DeviceSettingType.ClientRawP1Data:
						yield return new DeviceSettingsPayload
						{
							LastReceived = DateTime.Now.ToUniversalTime(),
							DeviceSetting = setting,
							Client = ProtocolNr >= 39 ? (QboxClient?)reader.ReadByte() : null,
							DeviceSettingValueStr = reader.ReadEncapsulatedString('"')
						};
						break;
					case DeviceSettingType.ClientPrimaryMeterType:
					case DeviceSettingType.ClientSecondaryMeterType:
					case DeviceSettingType.ClientFirmwareVersion:
						yield return new DeviceSettingsPayload
						{
							LastReceived = DateTime.Now.ToUniversalTime(),
							DeviceSetting = setting,
							Client = ProtocolNr >= 39 ? (QboxClient?)reader.ReadByte() : null,
							DeviceSettingValue = reader.ReadByte()
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
		private static DeviceSettingsPayload ParseCompositeZwave(int ProtocolNr, IProtocolReader reader)
		{
			var setting = reader.ReadByte();
			if (setting != 0x47)
				throw new ArgumentOutOfRangeException(String.Format("By 0x4D (Composite Zwave) Expected byte is: 0x47 found: {0:x2}", setting));
			var data = new ZWaveNetworkInfo();
			data.NumberOfDevices = reader.ReadByte();
			var nbrOfClientInfo = reader.ReadByte();
			for (int i = 0; i < nbrOfClientInfo; i++)
			{
				data.ClientInfos.Add(new ZWaveClientInfo
				{
					Info = reader.ReadEncapsulatedString('"')
				});
			}

			setting = reader.ReadByte();
			if (setting != (byte)DeviceSettingType.ClientZWaveNetworkInfoList)
				throw new ArgumentOutOfRangeException(String.Format("By 0x4D (Composite Zwave) Expected byte after client info's is: 0x60 found: {0:x2}", setting));

			var nbrOfDevices = reader.ReadByte();
			for (int i = 0; i < nbrOfDevices; i++)
			{
				data.Devices.Add(new ZWaveNetworkDevice
				{
					NodeId = reader.ReadByte(),
					DeviceType = reader.ReadByte()
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
			Logger.LogTrace("Enter");
			visitor.Accept(this);
		    Logger.LogTrace("Exit");
		}
	}
}