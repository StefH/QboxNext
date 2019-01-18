﻿using JetBrains.Annotations;
using System;

namespace QboxNext.Domain
{
    public class QboxState
    {
        public Guid CorrelationId { get; set; }

        public DateTime LogTime { get; set; }

        [NotNull]
        public string SerialNumber { get; set; }

        [NotNull]
        public string ProductNumber { get; set; }

        public string MessageType { get; set; }

        [CanBeNull]
        public string Message { get; set; }

        [NotNull]
        public string State { get; set; }

        public int? FirmwareVersion { get; set; }

        [CanBeNull]
        public string LastIpAddress { get; set; }

        public DateTime? LastSeen { get; set; }

        public DateTime? LastDataReceived { get; set; }

        public DateTime? LastElectricityConsumptionSeen { get; set; }

        public DateTime? LastElectricityGenerationSeen { get; set; }

        public DateTime? LastGasConsumptionSeen { get; set; }

        public DateTime? LastValidResponse { get; set; }

        public DateTime? LastInvalidResponse { get; set; }

        public DateTime? LastTimeIsReliable { get; set; }

        public DateTime? LastTimeUnreliable { get; set; }

        public DateTime? LastNotOperational { get; set; }

        public DateTime? LastHardReset { get; set; }

        public DateTime? LastPowerLoss { get; set; }

        public DateTime? LastImageValid { get; set; }

        public DateTime? LastImageInvalid { get; set; }

        public DateTime? LastTimeSynced { get; set; }

        public DateTime? LastPeak { get; set; }

        public DateTime? LastError { get; set; }

        [CanBeNull]
        public string LastErrorMessage { get; set; }
    }
}