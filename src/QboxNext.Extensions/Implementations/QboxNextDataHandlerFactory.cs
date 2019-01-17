using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Qboxes.Classes;
using Qboxes.Interfaces;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;

namespace QboxNext.Extensions.Implementations
{
    internal class QboxNextDataHandlerFactory : IQboxNextDataHandlerFactory
    {
        private readonly IAsyncStatusProvider _asyncStatusProvider;
        private readonly ILogger<QboxNextDataHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxNextDataHandlerFactory"/> class.
        /// </summary>
        /// <param name="asyncStatusProvider">The asynchronous status provider.</param>
        /// <param name="logger">The logger.</param>
        public QboxNextDataHandlerFactory([NotNull] IAsyncStatusProvider asyncStatusProvider, [NotNull] ILogger<QboxNextDataHandler> logger)
        {
            Guard.IsNotNull(asyncStatusProvider, nameof(asyncStatusProvider));
            Guard.IsNotNull(logger, nameof(logger));

            _asyncStatusProvider = asyncStatusProvider;
            _logger = logger;
        }

        /// <inheritdoc cref="IQboxNextDataHandlerFactory.Create(QboxDataDumpContext)"/>
        public QboxNextDataHandler Create(QboxDataDumpContext context)
        {
            Guard.IsNotNull(context, nameof(context));

            return new QboxNextDataHandler(context, _asyncStatusProvider, _logger);
        }
    }
}