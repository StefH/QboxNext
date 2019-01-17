using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QboxNext.Core.Dto;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Model;
using System;
using System.Threading.Tasks;

namespace QBoxNext.Business.Implementations
{
    internal class DefaultAsyncStatusProvider : IAsyncStatusProvider
    {
        private readonly ILogger<DefaultAsyncStatusProvider> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAsyncStatusProvider"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultAsyncStatusProvider([NotNull] ILogger<DefaultAsyncStatusProvider> logger)
        {
            Guard.IsNotNull(logger, nameof(logger));

            _logger = logger;
        }

        public Task StoreStatusAsync(
            Guid correlationId,
            QboxMessageType messageType, string message,
            string serialNumber, string productNumber,
            MiniState state, QboxStatus status)
        {
            _logger.LogInformation($"{correlationId}-{messageType}-{message}-{serialNumber}-{productNumber}-{state}-{JsonConvert.SerializeObject(status)}");

            return Task.CompletedTask;
        }
    }
}
