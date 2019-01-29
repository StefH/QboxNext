using JetBrains.Annotations;
using QboxNext.Core.Dto;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Extensions.Models.Public
{
    public class StateData
    {
        [NotNull]
        public string SerialNumber { get; set; }

        public QboxMessageType MessageType { get; set; }

        [CanBeNull]
        public string Message { get; set; }

        public MiniState State { get; set; }

        [CanBeNull]
        public QboxStatus Status { get; set; }
    }
}