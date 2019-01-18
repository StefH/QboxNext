using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using System;
using System.Threading.Tasks;

namespace QBoxNext.Business.Implementations
{
    internal class DefaultStateStoreService : IStateStoreService
    {
        private readonly ILogger<DefaultStateStoreService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStateStoreService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultStateStoreService([NotNull] ILogger<DefaultStateStoreService> logger)
        {
            Guard.IsNotNull(logger, nameof(logger));

            _logger = logger;
        }

        public Task StoreAsync(Guid correlationId, StateData stateData)
        {
            Guard.IsNotNull(stateData, nameof(stateData));

            _logger.LogInformation($"{correlationId}-{stateData.MessageType}-{stateData.Message}-{stateData.SerialNumber}-{stateData.ProductNumber}-{stateData.State}-{JsonConvert.SerializeObject(stateData.Status)}");

            return Task.CompletedTask;
        }
    }
}
