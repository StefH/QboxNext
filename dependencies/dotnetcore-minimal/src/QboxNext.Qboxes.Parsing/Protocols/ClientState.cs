namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// Introduced in release 32, not the same as MiniState!
    /// </summary>
    public enum ClientState 
    {
        WaitingForMeterType = 0,
        Operational = 1,
        ValidImageDownloaded = 2,
        InvalidImageDownloaded = 3,
        NA_4 = 4,
        NA_5 = 5,
        HardReset = 6,
        UnexpectedPowerdownOrReset = 7,
		UpgradeInProgress = 8,  // New in protocol 41 (R21)
		Unknown = 15,  // New in protocol 41 (R21)
    }
}
