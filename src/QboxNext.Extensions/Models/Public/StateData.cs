using System;
using JetBrains.Annotations;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Extensions.Models.Public
{
    public class StateData
    {
        [NotNull]
        public string SerialNumber { get; set; }

        public string MessageType { get; set; }

        [CanBeNull]
        public string Message { get; set; }

        public MiniState State { get; set; }

        [CanBeNull]
        public QboxStatus Status { get; set; }

        [CanBeNull]
        public DeviceMeterType? MeterType { get; set; }

        [CanBeNull]
        public DateTime? MessageTime { get; set; }

        [CanBeNull]
        public int? SequenceNumber { get; set; }

        [CanBeNull]
        public int? Payloads { get; set; }
    }
}