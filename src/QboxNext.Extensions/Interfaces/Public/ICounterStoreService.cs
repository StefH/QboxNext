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
        /// <param name="counters">correlationId + counterData</param>
        Task StoreAsync([NotNull] IList<(string correlationId, CounterData counterData)> counters);
    }
}