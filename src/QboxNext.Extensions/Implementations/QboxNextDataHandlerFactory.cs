using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Qboxes.Classes;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;

namespace QboxNext.Extensions.Implementations
{
    internal class QboxNextDataHandlerFactory : IQboxNextDataHandlerFactory
    {
        private readonly ICounterStoreService _counterStoreService;
        private readonly IStateStoreService _stateStoreService;
        private readonly ILogger<QboxNextDataHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxNextDataHandlerFactory"/> class.
        /// </summary>
        /// <param name="counterStoreService">The counter store service.</param>
        /// <param name="stateStoreService">The state store service.</param>
        /// <param name="logger">The logger.</param>
        public QboxNextDataHandlerFactory([NotNull] ICounterStoreService counterStoreService, [NotNull] IStateStoreService stateStoreService, [NotNull] ILogger<QboxNextDataHandler> logger)
        {
            Guard.IsNotNull(stateStoreService, nameof(stateStoreService));
            Guard.IsNotNull(stateStoreService, nameof(stateStoreService));
            Guard.IsNotNull(logger, nameof(logger));

            _counterStoreService = counterStoreService;
            _stateStoreService = stateStoreService;
            _logger = logger;
        }

        /// <inheritdoc cref="IQboxNextDataHandlerFactory.Create(string, QboxDataDumpContext)"/>
        public IQboxNextDataHandler Create(string correlationId, QboxDataDumpContext context)
        {
            Guard.IsNotNullOrEmpty(correlationId, nameof(correlationId));
            Guard.IsNotNull(context, nameof(context));

            return new QboxNextDataHandler(correlationId, context, _counterStoreService, _stateStoreService, _logger);
        }
    }
}