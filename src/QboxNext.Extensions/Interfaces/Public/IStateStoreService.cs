using JetBrains.Annotations;
using QboxNext.Extensions.Models.Public;
using System.Threading.Tasks;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface IStateStoreService
    {
        /// <summary>
        /// Stores the state.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="stateData">The StateData.</param>
        Task StoreAsync(string correlationId, [NotNull] StateData stateData);
    }
}