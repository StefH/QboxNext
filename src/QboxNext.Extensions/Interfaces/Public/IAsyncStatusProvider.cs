using JetBrains.Annotations;
using QboxNext.Core.Dto;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Model;
using System;
using System.Threading.Tasks;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface IAsyncStatusProvider
    {
        Task StoreStatusAsync(Guid correlationId, QboxMessageType messageType, [CanBeNull] string message, [NotNull] string serialNumber, [NotNull] string productNumber, MiniState state, [CanBeNull] QboxStatus status);
    }
}