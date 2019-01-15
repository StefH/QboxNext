using JetBrains.Annotations;
using QboxNext.Domain;
using QboxNext.Infrastructure.Azure.Models.Public;

namespace QboxNext.Infrastructure.Azure.Interfaces.Public
{
    public interface IMeasurementStoreService
    {
        /// <summary>
        /// Stores the specified measurement in Azure Tables.
        /// </summary>
        /// <param name="measurement">The measurement.</param>
        StoreResult Store([NotNull] Measurement measurement);
    }
}