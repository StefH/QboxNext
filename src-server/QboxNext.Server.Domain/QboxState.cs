using JetBrains.Annotations;
using System;

namespace QboxNext.Server.Domain
{
    public class QboxState
    {
        public string CorrelationId { get; set; }

        public DateTime LogTime { get; set; }

        [NotNull]
        public string SerialNumber { get; set; }

        public string MessageType { get; set; }

        [CanBeNull]
        public string Message { get; set; }

        [NotNull]
        public string State { get; set; }

        public int? FirmwareVersion { get; set; }

        [CanBeNull]
        public string MeterType { get; set; }

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
    }
}
