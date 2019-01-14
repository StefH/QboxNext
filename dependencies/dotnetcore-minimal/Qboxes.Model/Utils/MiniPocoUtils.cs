using QboxNext.Qserver.Core.Model;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Protocols;

namespace Qboxes.Utils
{
	public static class MiniPocoUtils
	{
		public static ClientMiniStatus GetClientMiniStatus(this MiniPoco _mini, ClientQboxPoco client)
		{
			if (client == null)
				return null;

			var clientKey = (QboxClient)client.ClientId;
			if (_mini.QboxStatus.ClientStatuses.ContainsKey(clientKey.ToString()))
				return new ClientMiniStatus(_mini.QboxStatus.ClientStatuses[clientKey.ToString()], _mini.QboxStatus.FirmwareVersion);

			return null;
		}
	}
}
