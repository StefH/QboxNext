using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Encryption;
using QboxNext.Model.Classes;
using QboxNext.Model.Interfaces;
using QboxNext.Storage;

namespace QboxNext.Qserver.Classes
{
    public class QboxDataDumpContextFactory : IQboxDataDumpContextFactory
    {
        private readonly ILogger<QboxDataDumpContextFactory> _logger;
        private readonly IMiniRetriever _miniRetriever;
        private readonly IStorageProviderFactory _storageProviderFactory;

        public QboxDataDumpContextFactory(ILogger<QboxDataDumpContextFactory> logger, IMiniRetriever miniRetriever, IStorageProviderFactory storageProviderFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _miniRetriever = miniRetriever ?? throw new ArgumentNullException(nameof(miniRetriever));
            _storageProviderFactory = storageProviderFactory ?? throw new ArgumentNullException(nameof(storageProviderFactory));
        }

        /// <summary>
        /// Overide to allow the creation of the Qbox Data Dump context object.
        /// It retrieves the url and external ip from the request and adds the request bytes to the context.
        /// After decrypting the content (if found to be encrypted) the QboxDataDump object is constructed.
        /// </summary>
        /// <param name="context">The controller context holding associations and objects in relation to the controller</param>
        /// <param name="pn">Product number</param>
        /// <param name="sn">Serial number</param>
        /// <returns>An object model that is requested in the bindingcontext</returns>
        public QboxDataDumpContext CreateContext(ControllerContext context, string pn, string sn)
        {
            try
            {
                int length;
                string lastSeenAtUrl;
                string externalIp;

                var bytes = GetRequestVariables(context, out length, out lastSeenAtUrl, out externalIp);

                var mini = _miniRetriever.Retrieve(sn);

                if (mini != null)
                {
                    mini.SetStorageProvider(_storageProviderFactory);
                    var message = QboxMessageDecrypter.DecryptPlainOrEncryptedMessage(bytes);
                    return new QboxDataDumpContext(message, length, lastSeenAtUrl, externalIp, mini, error: null);
                }

                return null;
            }
            catch (Exception e)
            {
                var s = string.Format("Serialnumber: {0} - orginal error message: {2} | {1}", sn, e.Message, pn);
                _logger.LogError(e, s);
                return new QboxDataDumpContext("N/A", 0, "N/A", "N/A", null, error: e.Message + " - " + s);//refactor: beter oplossen wordt nu gecontroleerd in de controller en die gooit een exception
            }
            finally
            {
                _logger.LogTrace("Return");
            }
        }

        /// <summary>
		/// Finds the values from the request and returnes them in out params for later use in the process.
		/// </summary>
		/// <param name="controllerContext">The Controller Context holds the information for the request and actual controller action</param>
		/// <param name="length">The length of the message in the request is returned using this out param</param>
		/// <param name="lastSeenAtUrl">The current request url is returned using this out param </param>
		/// <param name="externalIp">The external ip the mini is sending the request from</param>
		/// <returns></returns>
        private byte[] GetRequestVariables(ControllerContext controllerContext, out int length,
                                                  out string lastSeenAtUrl,
                                                  out string externalIp)
        {

            lastSeenAtUrl = controllerContext.HttpContext.Request.Host.Value;
            externalIp = controllerContext.HttpContext.Connection.RemoteIpAddress.ToString();

            var inputStream = controllerContext.HttpContext.Request.Body;
            length = (int)(controllerContext.HttpContext.Request.ContentLength ?? 0);

            return ReadRequestInputStreamFully(inputStream, length).ToArray();
        }

        /// <summary>
		/// Reads data from a stream until the end is reached. The
		/// data is returned as a byte array. An IOException is
		/// thrown if any of the underlying IO calls fail.
		/// </summary>
		/// <param name="stream">The stream to read data from</param>
		/// <param name="initialLength">The initial buffer length</param>
        public byte[] ReadRequestInputStreamFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            var buffer = new byte[initialLength];
            long read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, (int)read, (int)(buffer.Length - read))) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read != buffer.Length) continue;

                var nextByte = stream.ReadByte();

                // End of stream? If so, we're done
                if (nextByte == -1)
                {
                    return buffer;
                }

                // Nope. Resize the buffer, put in the byte we've just
                // read, and continue
                var newBuffer = new byte[buffer.Length * 2];
                Array.Copy(buffer, newBuffer, buffer.Length);
                newBuffer[read] = (byte)nextByte;
                buffer = newBuffer;
                read++;
            }
            // Buffer is now too big. Shrink it.
            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}