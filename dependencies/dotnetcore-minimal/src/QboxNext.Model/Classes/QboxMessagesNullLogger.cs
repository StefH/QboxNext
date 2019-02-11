using QboxNext.Core.Dto;
using QboxNext.Model.Interfaces;

namespace QboxNext.Model.Classes
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
