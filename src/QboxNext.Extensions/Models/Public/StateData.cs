using JetBrains.Annotations;
using QboxNext.Core.Dto;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Model;

namespace QboxNext.Extensions.Models.Public
{
    public class StateData
    {
        public QboxMessageType MessageType { get; set; }

        [CanBeNull] public string Message { get; set; }

        [NotNull] public string SerialNumber { get; set; }

        [NotNull] public string ProductNumber { get; set; }

        public MiniState State { get; set; }

        [CanBeNull] public QboxStatus Status { get; set; }
    }
}