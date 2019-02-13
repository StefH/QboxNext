using System;

namespace QboxNext.Qboxes.Parsing
{
	public class InvalidFormatException : Exception
	{
		public InvalidFormatException(string message)
			: base(message)
		{
		}
	}
}