using JetBrains.Annotations;
using QboxNext.Domain;
using QboxNext.Infrastructure.Azure.Models.Public;
using System.Threading.Tasks;

namespace QboxNext.Infrastructure.Azure.Interfaces.Public
{
    public interface IMeasurementStoreService
    {
        /// <summary>
        /// Stores the specified measurement in Azure Tables.
        /// </summary>
        /// <param name="measurement">The measurement.</param>
        Task<StoreResult> StoreAsync([NotNull] Measurement measurement);
    }
}