using System.Threading.Tasks;
using JetBrains.Annotations;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QBoxNext.Business.Interfaces.Internal
{
    internal interface IVisitorAsync : IVisitor
    {
        Task AcceptAsync([NotNull] CounterPayload payload);
    }
}