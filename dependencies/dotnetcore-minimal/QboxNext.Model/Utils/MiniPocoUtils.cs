using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Model.Utils
{
	public static class MiniPocoUtils
	{
		public static ClientMiniStatus GetClientMiniStatus(this Mini mini, ClientQbox client)
		{
			if (client == null)
				return null;

			var clientKey = (QboxClient)client.ClientId;
			if (mini.QboxStatus.ClientStatuses.ContainsKey(clientKey.ToString()))
				return new ClientMiniStatus(mini.QboxStatus.ClientStatuses[clientKey.ToString()], mini.QboxStatus.FirmwareVersion);

			return null;
		}
	}
}
