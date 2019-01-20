using Microsoft.Extensions.Logging;
using QboxNext.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    public class CounterPayload : BasePayload
    {
        private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger<CounterPayload>();

        public int InternalNr { get; set; }
        public ulong Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            Logger.LogTrace("Enter");
            visitor.Accept(this);
            Logger.LogTrace("Exit");
        }
    }
}