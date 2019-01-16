using JetBrains.Annotations;
using Qboxes.Classes;

namespace QboxNext.Handlers
{
    public interface IQboxNextDataHandlerFactory
    {
        /// <summary>
        /// Creates a QboxNextDataHandler based on specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        QboxNextDataHandler Create([NotNull] QboxDataDumpContext context);
    }
}