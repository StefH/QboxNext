
namespace QboxNext.Core.Utils
{
	/// <summary>
	/// The resolution for series supported by the Storage Provider(s)
	/// This determines the number of records and the cutoffs used in a period of time.
	/// </summary>
    public enum SeriesResolution
	{
		OneMinute = 1,
		FiveMinutes = 5,
		Hour = 60,
		Day = 1440,
		Week = 10080,
        Month = 43800 // gemiddelde van 365 dagen
	}
}
