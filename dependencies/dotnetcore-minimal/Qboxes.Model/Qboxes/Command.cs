using QboxNext.Qboxes.Parsing.Protocols;

namespace Qboxes
{
    using System.Text;
    using System;
    using System.Linq;
    using QboxNext.Qserver.Core.Interfaces;
    using QboxNext.Core.Utils;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

	/// <summary>
    /// refactor, mix of properties and enums at the moment.
    /// range 0x80 till 0x9F (properties) can changed in ActivityRequests
    /// </summary>
    public static class Command
    {
        public static string GetDescription(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DisplayAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DisplayAttribute)) as DisplayAttribute;
                    if (attr != null)
                    {
                        return attr.Name;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Default metersettings, used after state waiting from host or waiting for metertype from client
        /// </summary>
        /// <param name="meterType"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string DefaultMeterSettings(DeviceMeterType meterType, QboxClient client)
        {
            var cmd = new BaseParseResult();
            switch (meterType)
            {
                case DeviceMeterType.Ferraris_Black_Toothed:
                    // Set calibration to 10 minutes
					cmd.Write(RequestCalibrateMeter);
					cmd.Write((byte)10);
                    break;
                case DeviceMeterType.LED_TypeI:
                case DeviceMeterType.LED_TypeII:
                    cmd.Write((byte)DeviceSettingType.SensorChannel);
                    cmd.Write((byte)0x03);
                    if (client != QboxClient.None)
                    {
                        cmd.Write((byte)ClientActivityRequest.RequestToRestartClient);
                        cmd.Write((byte)client);
                    }
                    else
                    {
						cmd.Write(RequestRestart);
                    }
                    break;
            }
            return cmd.GetMessage();
        }


		/// <summary>
		/// Encode bytes to strings appropriate for putting in the response message.
		/// </summary>
		public static string Encode(params byte[] inBytes)
		{
			return BaseParseResult.HexStr(inBytes);
		}


		public const byte RequestUpgradeFirmware = 0x80;
		public const byte RequestRestart = 0x81;
		public const byte RequestRestartWithFactoryDefaults = 0x82;
		public const byte RequestCalibrateMeter = 0x84;
		public const byte RequestDeviceSetting = 0x85;
    }
}
// Note 1:
// "After a restart with Factory Defaults (RFD) of the HOST, the complete configuration must be send again. 
// Note that a RFD could be triggered by a user or by the watchdog, so the server should monitor the status flag of the host to detect a RFD (value 6). Actions required after RFD (host already gets meter type):
// a. If qbox has 1 or more clients: 
//    For each client set the stored meter type again without a restart (only host needs to be told of the client configuration again; clients don't need to be restarted or reconfigured)
// b. When single qbox or host has meter type LED: Set all LED settings (like channel) if they were explicitly set before (restart after channel set)
// c. When single qbox or host has meter type Ferraris: Set all Ferraris settings if they were explicitly set before (restart after all callibration values)"
