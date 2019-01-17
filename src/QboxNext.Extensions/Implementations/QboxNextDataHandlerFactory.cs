﻿using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Qboxes.Classes;
using Qboxes.Interfaces;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;

namespace QboxNext.Extensions.Implementations
{
    internal class QboxNextDataHandlerFactory : IQboxNextDataHandlerFactory
    {
        private readonly IQboxMessagesLogger _qboxMessagesLogger;
        private readonly ILogger<QboxNextDataHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxNextDataHandlerFactory"/> class.
        /// </summary>
        /// <param name="qboxMessagesLogger">The qbox messages logger.</param>
        /// <param name="logger">The logger.</param>
        public QboxNextDataHandlerFactory([NotNull] IQboxMessagesLogger qboxMessagesLogger, [NotNull] ILogger<QboxNextDataHandler> logger)
        {
            Guard.IsNotNull(qboxMessagesLogger, nameof(qboxMessagesLogger));
            Guard.IsNotNull(logger, nameof(logger));

            _qboxMessagesLogger = qboxMessagesLogger;
            _logger = logger;
        }

        /// <inheritdoc cref="IQboxNextDataHandlerFactory.Create(QboxDataDumpContext)"/>
        public QboxNextDataHandler Create(QboxDataDumpContext context)
        {
            Guard.IsNotNull(context, nameof(context));

            return new QboxNextDataHandler(context, _qboxMessagesLogger, _logger);
        }
    }
}