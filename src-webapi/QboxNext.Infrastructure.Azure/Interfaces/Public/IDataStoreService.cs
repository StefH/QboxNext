using JetBrains.Annotations;
using QboxNext.Domain;
using QboxNext.Infrastructure.Azure.Models.Public;
using System.Threading.Tasks;

namespace QboxNext.Infrastructure.Azure.Interfaces.Public
{
    public interface IDataStoreService
    {
        /// <summary>
        /// Stores the specified measurement in Azure Tables.
        /// </summary>
        /// <param name="qboxMeasurement">The measurement.</param>
        Task<StoreResult> StoreAsync([NotNull] QboxMeasurement qboxMeasurement);

        /// <summary>
        /// Stores the specified state in Azure Tables.
        /// </summary>
        /// <param name="qboxState">The state.</param>
        Task<StoreResult> StoreAsync([NotNull] QboxState qboxState);
    }
}