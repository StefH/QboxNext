using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Encryption;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Internal;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Model.Classes;
using System;

namespace QboxNext.Extensions.Implementations
{
    internal class QboxDataDumpContextFactory : IQboxDataDumpContextFactory
    {
        private readonly ILogger<QboxDataDumpContextFactory> _logger;
        private readonly IMiniPocoFactory _miniPocoFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxDataDumpContextFactory"/> class.
        /// </summary>
        /// <param name="miniPocoFactory">The mini poco factory.</param>
        /// <param name="logger">The logger.</param>
        public QboxDataDumpContextFactory([NotNull] IMiniPocoFactory miniPocoFactory, [NotNull] ILogger<QboxDataDumpContextFactory> logger)
        {
            Guard.IsNotNull(miniPocoFactory, nameof(miniPocoFactory));
            Guard.IsNotNull(logger, nameof(logger));

            _miniPocoFactory = miniPocoFactory;
            _logger = logger;
        }

        /// <inheritdoc cref="IQboxDataDumpContextFactory.Create(QboxContext)"/>
        public QboxDataDumpContext Create(QboxContext context)
        {
            Guard.IsNotNull(context, nameof(context));

            try
            {
                string lastSeenAtUrl = context.LastSeenAtUrl;
                string externalIp = context.ExternalIp;

                var message = QboxMessageDecrypter.DecryptPlainOrEncryptedMessage(context.Message);
                int length = context.Message.Length;

                var mini = _miniPocoFactory.Create(context.SerialNumber, context.ProductNumber);

                return new QboxDataDumpContext(message, length, lastSeenAtUrl, externalIp, mini);
            }
            catch (Exception e)
            {
                string errorMessage = $"SerialNumber: {context.SerialNumber} ProductNumber: {context.ProductNumber} original error message: {e.Message}";
                _logger.LogError(e, errorMessage);

                return new QboxDataDumpContext("N/A", 0, "N/A", "N/A", null, e.Message + " - " + errorMessage); //refactor: beter oplossen wordt nu gecontroleerd in de controller en die gooit een exception
            }
        }
    }
}