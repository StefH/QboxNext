using JetBrains.Annotations;
using QboxNext.Extensions.Models.Public;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface ICounterStoreService
    {
        /// <summary>
        /// Stores the Counter.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="counterData">The CounterData.</param>
        Task StoreAsync(string correlationId, [NotNull] CounterData counterData);

        /// <summary>
        /// Stores multiple Counters.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="counters">The CounterData.</param>
        Task StoreAsync([NotNull] string correlationId, [NotNull] IList<CounterData> counters);
    }
}