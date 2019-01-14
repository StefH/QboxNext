using System;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Model;

namespace Qboxes.Classes
{
    public class QboxDataDumpContext
    {        
        public string Message { get; private set; }
        public int Length { get; private set; }
        public MiniPoco Mini { get; private set; }
        public string Error { get; set; }

        /// <summary>
        /// Constructs the data for the handling of a qbox data dump
        /// </summary>        
        /// <param name="message">The actual message dumped by the Qbox. This is the decrypted message!</param>
        /// <param name="length">The length of the message</param>
        /// <param name="lastSeenAtUrl">The url used to dump the message</param>
        /// <param name="externalIp">The external ip from which the dump was initiated</param>
        /// <param name="mini">The mini the dump is made from</param>
        /// <param name="error">An optional error message to show if the retrieval or binding of the Mini, MiniPoco or decryption of the message was incorrect.</param>
        public QboxDataDumpContext(string message, int length, string lastSeenAtUrl, string externalIp, MiniPoco mini, string error = null)
        {            
            Guard.IsNotNullOrEmpty(lastSeenAtUrl, "last seen url is missing");
            Guard.IsNotNullOrEmpty(externalIp, "External Ip is missing");
            Guard.IsNotNullOrEmpty(message, "message is missing");            
            
            Message = message;
            Length = length;
            Mini = mini;
            Error = error;
            if (Mini != null)
            {
                Mini.QboxStatus.IpAddress[externalIp] = DateTime.Now.ToUniversalTime();
                Mini.QboxStatus.Url = lastSeenAtUrl;
            }
        }
    }
}
