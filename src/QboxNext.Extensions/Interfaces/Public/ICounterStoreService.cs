using JetBrains.Annotations;
using QboxNext.Extensions.Models.Public;
using System.Threading.Tasks;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface ICounterStoreService
    {
        /// <summary>
        /// Stores the state.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="counterData">The CounterData.</param>
        Task StoreAsync(string correlationId, [NotNull] CounterData counterData);
    }
}