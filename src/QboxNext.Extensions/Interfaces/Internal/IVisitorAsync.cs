using JetBrains.Annotations;
using QboxNext.Qboxes.Parsing.Protocols;
using System;
using System.Threading.Tasks;

namespace QboxNext.Extensions.Interfaces.Internal
{
    public interface IVisitorAsync : IVisitor
    {
        /// <summary>
        /// Process the payload
        /// </summary>
        /// <param name="correlationId">The correlation Id</param>
        /// <param name="payload">Payload, InternalNr will be mapped to the actual internal number used to store the data.</param>
        Task AcceptAsync(Guid correlationId, [NotNull] CounterPayload payload);
    }
}