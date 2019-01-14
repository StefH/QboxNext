using Qboxes.Interfaces;
using QboxNext.Core.Dto;


namespace Qboxes.Classes
{
	/// <summary>
	/// QboxMessages logger die niks doet.
	/// </summary>
	public class QboxMessagesNullLogger : IQboxMessagesLogger
	{
		public void LogQboxMessage(string serial, string message, QboxMessageType messageType)
		{
		}
	}
}
