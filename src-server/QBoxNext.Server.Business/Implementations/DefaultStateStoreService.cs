﻿using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Model.Qboxes;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class DefaultStateStoreService : IStateStoreService
    {
        private readonly IAzureTablesService _azureTablesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStateStoreService"/> class.
        /// </summary>
        /// <param name="azureTablesService">The state store service.</param>
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
                MessageType = stateData.MessageType,
                State = stateData.State.ToString(),
                FirmwareVersion = stateData.Status?.FirmwareVersion,
                SequenceNumber = stateData.SequenceNumber,
                LastIpAddress = stateData.Status?.LastIpAddress.Key,
                LastIpAddressUpdate = stateData.Status?.LastIpAddress.Value,
                MeterType = stateData.MeterType != null ? stateData.MeterType.ToString() : null,
                Payloads = stateData.Payloads,
                MessageTime = stateData.MessageTime
            };

            if (stateData.Message != null)
            {
                if (stateData.Message.AsSpan(1, 10).ToArray().All(char.IsLetterOrDigit))
                {
                    state.Message = stateData.Message;
                }
                else
                {
                    // Convert to Base64 string
                    state.Message = $"Base64 byte[] {Convert.ToBase64String(Encoding.UTF8.GetBytes(stateData.Message))}";
                }
            }

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
