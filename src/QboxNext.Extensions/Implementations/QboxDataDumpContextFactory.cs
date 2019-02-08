using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Encryption;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Internal;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Model.Classes;

namespace QboxNext.Extensions.Implementations
{
    internal class QboxDataDumpContextFactory : IQboxDataDumpContextFactory
    {
        private readonly ILogger<QboxDataDumpContextFactory> _logger;
        private readonly IQboxMiniFactory _qboxMiniFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxDataDumpContextFactory"/> class.
        /// </summary>
        /// <param name="qboxMiniFactory">The mini poco factory.</param>
        /// <param name="logger">The logger.</param>
        public QboxDataDumpContextFactory([NotNull] IQboxMiniFactory qboxMiniFactory, [NotNull] ILogger<QboxDataDumpContextFactory> logger)
        {
            Guard.IsNotNull(qboxMiniFactory, nameof(qboxMiniFactory));
            Guard.IsNotNull(logger, nameof(logger));

            _qboxMiniFactory = qboxMiniFactory;
            _logger = logger;
        }

        /// <inheritdoc cref="IQboxDataDumpContextFactory.Create(QboxContext)"/>
        public QboxDataDumpContext Create(QboxContext context)
        {
            Guard.IsNotNull(context, nameof(context));

            string lastSeenAtUrl = context.LastSeenAtUrl;
            string externalIp = context.ExternalIp;

            var message = QboxMessageDecrypter.DecryptPlainOrEncryptedMessage(context.Message);
            int length = context.Message.Length;

            var mini = _qboxMiniFactory.Create(context.SerialNumber);

            return new QboxDataDumpContext(message, length, lastSeenAtUrl, externalIp, mini);
        }
    }
}