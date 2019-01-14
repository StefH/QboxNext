using System.ComponentModel.DataAnnotations;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    public enum DeviceSettingType
    {
        // device setting read/write
        [Display(Name = "URL to download firmware images from")]
        FirmwareUrl = 0x01,
        [Display(Name = "URL to report metering data to")]
        ReportUrl = 0x02,
        [Display(Name = "Primary meter type (see below)")]
        PrimaryMeterType = 0x03,
        [Display(Name = "Secondary meter type (see below)")]
        SecondaryMeterType = 0x04,
        [Display(Name = "Manufacturer meter type")]
        ManufacturerMeterType = 0x05,
        [Display(Name = "Sensor 1 low")]
        Sensor1Low = 0x06,
        [Display(Name = "Sensor 1 high")]
        Sensor1High = 0x07,
        [Display(Name = "Sensor 2 low")]
        Sensor2Low = 0x08,
        [Display(Name = "Sensor 2 high")]
        Sensor2High = 0x09,
        [Display(Name = "Calibration settings composite")]
        CalibrationSettingsComposite = 0x0A,
        [Display(Name = "LED sensor channel")]
        SensorChannel = 0x20,
        [Display(Name = "LED sensor minimum pulse length")]
        SensorMinPulseLength = 0x21,
        [Display(Name = "LED sensor minimum pulse width")]
        SensorMinPulseWidth = 0x22,
        [Display(Name = "LED sensor maximum pulse width")]
        SensorMaxPulseWidth = 0x23,
        [Display(Name = "LED sensor minimum gap")]
        SensorMinGap = 0x24,
        [Display(Name = "LED sensor maximum gap")]
        SensorMaxGap = 0x25,
        [Display(Name = "LED sensor noise level")]
        SensorNoiseLevel = 0x26,
        [Display(Name = "LED sensor filter coefficient")]
        SensorFilterCoefficient = 0x27,
        [Display(Name = "LED sensor settings")]
        SensorSettingsComposite = 0x28,

        // device info read only
        [Display(Name = "LED sensor pulse length minimum")]
        SensorPulseLengthMin = 0x29,
        [Display(Name = "LED sensor pulse length maximum")]
        SensorPulseLengthMax = 0x2A,
        [Display(Name = "LED sensor pulse width minimum")]
        SensorPulseWidthMin = 0x2B,
        [Display(Name = "LED sensor pulse width maximum")]
        SensorPulseWidthMax = 0x2C,
        [Display(Name = "LED sensor baseline minimum")]
        SensorBaselineMin = 0x2D,
        [Display(Name = "LED sensor baseline maximum")]
        SensorBaselineMax = 0x2E,
        [Display(Name = "LED sensor signal minimum")]
        SensorSignalMin = 0x2F,
        [Display(Name = "LED sensor signal maximum")]
        SensorSignalMax = 0x30,
        [Display(Name = "LED sensor signal average")]
        SensorSignalAverage = 0x31,
        [Display(Name = "LED sensor measurements (last minute)")]
        SensorMeasurements = 0x32,
		[Display(Name = "P1 Manufacturer Code")]
		P1ManufacturerCode = 0x50,  // device info read only
		[Display(Name = "P1 DSMR Version")]
		P1CounterOptions = 0x51,  // New in protocol 41
		[Display(Name = "P1 DSMR Version")]
		P1DSMRVersion = 0x52,  // New in protocol 44

		// Client settings
		[Display(Name = "Client primary meter type")]
        ClientPrimaryMeterType = 0x41,  // client setting write only
        [Display(Name = "Client secondary meter type")]
        ClientSecondaryMeterType = 0x42,  // client setting write only

		// Client info
		[Display(Name = "Client product and serial number (PN/SN)")]
		ClientProductAndSerialNumber = 0x40,  //  client info read only
        [Display(Name = "Client firmware version")]
        ClientFirmwareVersion = 0x43,  // client info read only
        [Display(Name = "Client’s Raw P1 Data")]
        ClientRawP1Data = 0x44,  // client info read only
		// New in protocol 43
		[Display(Name = "Client Manufacturer report")]
		ClientManufacturerReport = 0x45,
		[Display(Name = "Node ID of the client")]
		ClientNodeId = 0x46,
		[Display(Name = "Number of devices in the ZWave network")]
		ClientNumberOfDevicesInZWaveNetwork = 0x47,
		[Display(Name = "Counter types (energy) supported by the Smart Plug")]
		ClientCounterTypesSmartPlug = 0x48,
		[Display(Name = "Contains Info from 45... to.. 48 depending on the device added")]
		ClientCompositeZWave = 0x4D,
		[Display(Name = "Raw Node Information Frame from a particular node in the network")]
		ClientRawNodeInformation = 0x4E,
		[Display(Name = "Raw Supported Report from Meter Command Class from a particular node in the network")]
		ClientRawSupportedReport = 0x4F,

		// Node info
		[Display(Name = "Z-Wave network info list")]
		ClientZWaveNetworkInfoList = 0x60,
		[Display(Name = "Meter Report value (each 4 bytes)")]
		ClientZWaveMeterReport = 0x61,
		[Display(Name = "Basic Set setting (0x00 is OFF and 0xFF is ON)")]
		ClientZwaveBasicSetSetting = 0x62,

		// Client Activity requests
		[Display(Name = "Client product and serial number (PN/SN) request")]
		ClientActivityRequestProductAndSerialNumber = 0xA3,
		[Display(Name = "Client firmware version request")]
		ClientActivityRequestFirmwareVersion = 0xA6,
    }
}