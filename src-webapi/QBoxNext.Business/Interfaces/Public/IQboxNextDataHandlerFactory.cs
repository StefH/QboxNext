using JetBrains.Annotations;
using Qboxes.Classes;
using QBoxNext.Business.Implementations;

namespace QBoxNext.Business.Interfaces.Public
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