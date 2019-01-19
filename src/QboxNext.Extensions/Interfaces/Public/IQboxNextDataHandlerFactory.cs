using JetBrains.Annotations;
using Qboxes.Classes;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface IQboxNextDataHandlerFactory
    {
        /// <summary>
        /// Creates a QboxNextDataHandler based on specified <see cref="QboxDataDumpContext"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="correlationId">The correlationId.</param>
        IQboxNextDataHandler Create([NotNull] string correlationId, [NotNull] QboxDataDumpContext context);
    }
}