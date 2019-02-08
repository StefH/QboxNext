using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using System;
using System.Linq;
using WindowsAzure.Table.Attributes;

namespace QboxNext.Server.Infrastructure.Azure.Models.Internal
{
    public class StateEntity
    {
        [PartitionKey]
        public string SerialNumber { get; set; }

        [RowKey]
        public string RowKey { get; set; }

        public string CorrelationId { get; set; }

        public DateTime LogTimeStamp { get; set; }

        public string MessageType { get; set; }

        public string Message { get; set; }

        public string State { get; set; }

        public int? FirmwareVersion { get; set; }

        public DateTime? LastDataReceived { get; set; }
        public DateTime? LastElectricityConsumptionSeen { get; set; }
        public DateTime? LastElectricityGenerationSeen { get; set; }
        public DateTime? LastError { get; set; }
        public DateTime? LastGasConsumptionSeen { get; set; }

        public DateTime? LastHardReset { get; set; }

        public DateTime? LastImageInvalid { get; set; }

        public DateTime? LastImageValid { get; set; }

        public DateTime? LastInvalidResponse { get; set; }

        public string LastErrorMessage { get; set; }

        public string LastIpAddress { get; set; }

        public DateTime? LastIpAddressUpdate { get; set; }

        public DateTime? LastNotOperational { get; set; }

        public DateTime? LastPeak { get; set; }

        public DateTime? LastPowerLoss { get; set; }

        public DateTime? LastSeen { get; set; }

        public DateTime? LastTimeIsReliable { get; set; }

        public DateTime? LastTimeSynced { get; set; }

        public DateTime? LastTimeUnreliable { get; set; }

        public DateTime? LastValidResponse { get; set; }

        public StateEntity()
        {
            // This is needed.
        }

        public StateEntity([NotNull] QboxState qboxState)
        {
            Guard.NotNull(qboxState, nameof(qboxState));

            SerialNumber = qboxState.SerialNumber;
            RowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:d19}:{qboxState.MessageType}";

            CorrelationId = qboxState.CorrelationId;
            LogTimeStamp = qboxState.LogTime;
            MessageType = qboxState.MessageType;
            Message = qboxState.Message;
            State = qboxState.State;
            FirmwareVersion = qboxState.FirmwareVersion;

            // Copy all 'Last...' values
            var propertiesToCopy = typeof(QboxState).GetProperties()
                .Where(pi => pi.Name.StartsWith("Last") && (pi.PropertyType == typeof(string) || pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
            );
            foreach (var propertyInfo in propertiesToCopy)
            {
                var value = propertyInfo.GetValue(qboxState);
                var targetPropertyInfo = typeof(StateEntity).GetProperty(propertyInfo.Name);

                // Skip null values
                if (targetPropertyInfo != null && value != null)
                {
                    targetPropertyInfo.SetValue(this, value);
                }
            }
        }
    }
}
