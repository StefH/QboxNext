using System.IO;
using Microsoft.AspNetCore.Mvc;
using Qboxes.Classes;

namespace QboxNext.Qserver.Classes
{
    public interface IQboxDataDumpContextFactory
    {
        /// <summary>
        /// Overide to allow the creation of the Qbox Data Dump context object.
        /// It retrieves the url and external ip from the request and adds the request bytes to the context.
        /// After decrypting the content (if found to be encrypted) the QboxDataDump object is constructed.
        /// </summary>
        /// <param name="context">The controller context holding associations and objects in relation to the controller</param>
        /// <param name="pn">Product number</param>
        /// <param name="sn">Serial number</param>
        /// <returns>An object model that is requested in the bindingcontext</returns>
        QboxDataDumpContext CreateContext(ControllerContext context, string pn, string sn);

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        byte[] ReadRequestInputStreamFully(Stream stream, int initialLength);
    }
}