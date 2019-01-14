namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// State for the mini
    /// </summary>
    public enum MiniState 
    {    
        Waiting = 0,
        Operational = 1,
        ValidImage = 4,
        InvalidImage = 5,
        HardReset = 6,
        UnexpectedReset = 7,
    }
}