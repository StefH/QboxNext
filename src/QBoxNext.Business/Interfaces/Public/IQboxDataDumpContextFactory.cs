using JetBrains.Annotations;
using Qboxes.Classes;
using QBoxNext.Business.Models;

namespace QBoxNext.Business.Interfaces.Public
{
    public interface IQboxDataDumpContextFactory
    {
        /// <summary>
        /// Override to allow the creation of the Qbox Data Dump context object.
        /// It retrieves the url and external ip from the request and adds the request bytes to the context.
        /// After decrypting the content (if found to be encrypted) the QboxDataDump object is constructed.
        /// </summary>
        /// <param name="context">The QboxContext holding associations and objects in relation to the controller</param>
        /// <returns>An object model that is requested in the bindingcontext</returns>
        QboxDataDumpContext CreateContext([NotNull] QboxContext context);
    }
}