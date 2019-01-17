using System.Threading.Tasks;
using JetBrains.Annotations;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Extensions.Interfaces.Internal
{
    public interface IVisitorAsync : IVisitor
    {
        Task AcceptAsync([NotNull] CounterPayload payload);
    }
}