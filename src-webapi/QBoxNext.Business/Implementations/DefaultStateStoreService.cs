using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Utils;
using QboxNext.Domain;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Qserver.Core.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QBoxNext.Business.Implementations
{
    internal class DefaultStateStoreService : IStateStoreService
    {
        [NotNull] private readonly IDataStoreService _dataStoreService;
        private readonly ILogger<DefaultStateStoreService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStateStoreService"/> class.
        /// </summary>
        /// <param name="dataStoreService">The data store service.</param>
        /// <param name="logger">The logger.</param>
        public DefaultStateStoreService([NotNull] IDataStoreService dataStoreService, [NotNull] ILogger<DefaultStateStoreService> logger)
        {
            Guard.IsNotNull(dataStoreService, nameof(dataStoreService));
            Guard.IsNotNull(logger, nameof(logger));

            _dataStoreService = dataStoreService;
            _logger = logger;
        }

        public async Task StoreAsync(Guid correlationId, StateData stateData)
        {
            Guard.IsNotNull(stateData, nameof(stateData));

            var state = new QboxState
            {
                CorrelationId = correlationId,
                LogTime = DateTime.UtcNow,
                SerialNumber = stateData.SerialNumber,
                ProductNumber = stateData.ProductNumber,
                MessageType = stateData.MessageType.ToString(),
                Message = stateData.Message,
                State = stateData.State.ToString(),
                FirmwareVersion = stateData.Status?.FirmwareVersion
            };

            // Copy all 'Last...' values
            if (stateData.Status != null)
            {
                var propertiesToCopy = typeof(QboxStatus).GetProperties()
                    .Where(pi => pi.Name.StartsWith("Last") && (pi.PropertyType == typeof(string) || pi.PropertyType == typeof(DateTime))
                );
                foreach (var propertyInfo in propertiesToCopy)
                {
                    var value = propertyInfo.GetValue(stateData.Status);
                    var targetPropertyInfo = typeof(QboxState).GetProperty(propertyInfo.Name);

                    // Skip default DateTime values
                    if (targetPropertyInfo != null && !(value is DateTime date && date == default(DateTime)))
                    {
                        targetPropertyInfo.SetValue(state, value);
                    }
                }
            }

            await _dataStoreService.StoreAsync(state);
        }
    }
}
