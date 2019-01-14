namespace QboxNext.Qboxes.Parsing.Protocols
{
    public enum DeviceMeterType 
    {    
        LED_TypeI = 0,    
        LED_TypeII = 1,
        Ferraris_Black_Toothed = 2,
        Smart_Meter_E = 6,
        Smart_Meter_EG = 7,
        SO_Pulse = 8,
        Soladin_600 = 9,
		SmartPlug = 0x1C,  // New in protocol 41, internal use
		ZWaveMeter = 0x1D,  // New in protocol 41, internal use
        NO_Meter = 0x1E,  // (e.g. in case of Z-wave host not connected to meters)
        //NO_Meter = 0x1F,  // (release 32 (obsolete / not in production)
        None = 255,
    }
}