using System.ComponentModel.DataAnnotations;

namespace Qboxes
{
	public enum ClientActivityRequest
	{
		[Display(Name = "Replicate firmware to client(s)")]
		ReplicateFirmwareToClient = 0xA0,
		[Display(Name = "Request to restart client(s)")]
		RequestToRestartClient = 0xA1,
		[Display(Name = "Request to restart client(s) with factory defaults")]
		RequestToRestartClientWithFactoryDefaults = 0xA2,
		[Display(Name = "Request to report product and serial number (PN/SN)")]
		RequestToReportPNSN = 0xA3,
		[Display(Name = "Request for client primary meter type")]
		RequestForClientPrimaryMeterType = 0xA4,
		[Display(Name = "Request for client secondary meter type")]
		RequestForClientSecondaryMeterType = 0xA5,
		[Display(Name = "Request for Client Firmware version")]
		RequestForClientFirmwareVersion = 0xA6,
		// New in protocol 39
		[Display(Name = "Request to provide raw P1 data from the Client")]
		RequestRawP1Data = 0xA7,
		// New in protocol 43
		[Display(Name = "Request the list of the clients info")]
		RequestListOfClientInfo = 0xA8,
		[Display(Name = "Request the Node Information Frame")]
		RequestNodeInformationFrame = 0xA9,
		[Display(Name = "Request the supported meter counters")]
		RequestSupportedMeterCounters = 0xAA
	}
}