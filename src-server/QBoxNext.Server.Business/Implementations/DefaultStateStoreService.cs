using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Qserver.Core.Model;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class DefaultStateStoreService : IStateStoreService
    {
        private readonly IAzureTablesService _azureTablesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStateStoreService"/> class.
        /// </summary>
        /// <param name="azureTablesService">The data store service.</param>
        public DefaultStateStoreService([NotNull] IAzureTablesService azureTablesService)
        {
            Guard.IsNotNull(azureTablesService, nameof(azureTablesService));

            _azureTablesService = azureTablesService;
        }

        /// <inheritdoc cref="IStateStoreService.StoreAsync(string, StateData)"/>
        public async Task StoreAsync(string correlationId, StateData stateData)
        {
            Guard.IsNotNullOrEmpty(correlationId, nameof(correlationId));
            Guard.IsNotNull(stateData, nameof(stateData));

            var state = new QboxState
            {
                CorrelationId = correlationId,
                LogTime = DateTime.UtcNow,
                SerialNumber = stateData.SerialNumber,
                MessageType = stateData.MessageType.ToString(),
                Message = stateData.Message,
                State = stateData.State.ToString(),
                FirmwareVersion = stateData.Status?.FirmwareVersion,
                LastIpAddress = stateData.Status?.LastIpAddress.Key,
                LastIpAddressUpdate = stateData.Status?.LastIpAddress.Value
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

            await _azureTablesService.StoreAsync(state);
        }
    }
}
