using JetBrains.Annotations;
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
        private readonly IDataStoreService _dataStoreService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStateStoreService"/> class.
        /// </summary>
        /// <param name="dataStoreService">The data store service.</param>
        /// <param name="logger">The logger.</param>
        public DefaultStateStoreService([NotNull] IDataStoreService dataStoreService)
        {
            Guard.IsNotNull(dataStoreService, nameof(dataStoreService));

            _dataStoreService = dataStoreService;
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
