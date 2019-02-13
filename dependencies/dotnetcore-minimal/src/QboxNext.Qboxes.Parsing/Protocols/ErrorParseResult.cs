namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class ErrorParseResult : BaseParseResult
	{
		public ErrorParseResult(BaseParseResult baseParseResult)
		{
			SequenceNr = baseParseResult.SequenceNr;
			ProtocolNr = baseParseResult.ProtocolNr;
		}

		public string Error { get; set; }
	}
}