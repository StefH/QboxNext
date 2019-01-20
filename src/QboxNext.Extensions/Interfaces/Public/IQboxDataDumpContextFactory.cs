using JetBrains.Annotations;
using QboxNext.Extensions.Models.Public;
using QboxNext.Model.Classes;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface IQboxDataDumpContextFactory
    {
        /// <summary>
        /// Override to allow the creation of the Qbox Data Dump context object.
        /// It retrieves the url and external ip from the request and adds the request bytes to the context.
        /// After decrypting the content (if found to be encrypted) the QboxDataDump object is constructed.
        /// </summary>
        /// <param name="context">The QboxContext holding associations and objects in relation to the controller</param>
        /// <returns>QboxDataDumpContext</returns>
        QboxDataDumpContext Create([NotNull] QboxContext context);
    }
}