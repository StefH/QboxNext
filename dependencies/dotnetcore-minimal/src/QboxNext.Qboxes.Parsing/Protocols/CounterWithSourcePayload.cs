namespace QboxNext.Qboxes.Parsing.Protocols
{
	/// <summary>
	/// R16: New properties 
	/// </summary>
	public class CounterWithSourcePayload : CounterPayload
	{
		public CounterSource Source { get; set; }
		public bool PrimaryMeter { get; set; }
	}
}