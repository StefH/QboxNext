using System;
using Newtonsoft.Json;
using QboxNext.Core.Logging;

namespace QboxNext.Core.Utils
{
    public class JsonUtils
    {
        public static string ObjectToJsonString(object inData)
        {
            try
            {
                return JsonConvert.SerializeObject(inData, Formatting.Indented);
            }
            catch (Exception e)
            {
                Log.ErrorException(String.Format("ObjectToJsonString: {0}", inData.GetType().Name), e);
                return String.Format("Error serializing object to json: {0}", e.Message);
            }
        }

        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
    }
}
