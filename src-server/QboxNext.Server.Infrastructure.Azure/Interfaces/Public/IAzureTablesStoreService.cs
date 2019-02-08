using JetBrains.Annotations;
using QboxNext.Server.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QboxNext.Server.Infrastructure.Azure.Interfaces.Public
{
    public interface IAzureTablesStoreService
    {
        /// <summary>
        /// Stores the specified measurement in Azure Tables.
        /// </summary>
        /// <param name="qboxMeasurement">The measurement.</param>
        Task<bool> StoreAsync([NotNull] QboxMeasurement qboxMeasurement);

        /// <summary>
        /// Stores the specified measurements as a Batch operation in Azure Tables.
        /// </summary>
        /// <param name="qboxMeasurements">The measurements.</param>
        Task<bool> StoreBatchAsync([NotNull] IList<QboxMeasurement> qboxMeasurements);

        /// <summary>
        /// Stores the specified state in Azure Tables.
        /// </summary>
        /// <param name="qboxState">The state.</param>
        Task<bool> StoreAsync([NotNull] QboxState qboxState);
    }
}