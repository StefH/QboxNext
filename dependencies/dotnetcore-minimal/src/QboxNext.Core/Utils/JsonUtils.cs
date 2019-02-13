using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using QboxNext.Logging;

namespace QboxNext.Core.Utils
{
    public static class JsonUtils
    {
        private static readonly ILogger Log = QboxNextLogProvider.CreateLogger("JsonUtils");

        public static string ObjectToJsonString(object inData)
        {
            try
            {
                Log.LogInformation("test");
                return JsonConvert.SerializeObject(inData, Formatting.Indented);
            }
            catch (Exception e)
            {
                Log.LogError(e, "ObjectToJsonString: {0}", inData.GetType().Name);
                return $"Error serializing object to json: {e.Message}";
            }
        }
    }
}