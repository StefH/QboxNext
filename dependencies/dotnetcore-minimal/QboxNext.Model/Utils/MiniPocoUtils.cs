using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Model;

namespace QboxNext.Model.Utils
{
	public static class MiniPocoUtils
	{
		public static ClientMiniStatus GetClientMiniStatus(this MiniPoco mini, ClientQboxPoco client)
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
